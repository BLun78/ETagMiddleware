using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace BLun.ETagMiddleware.Common
{
    /// <summary>
    /// Enables ETag middleware for request
    /// </summary>
    internal abstract class ETagCache
    {
        protected const string NoContentBodyHash = "\"z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg\"";
        protected readonly ILogger Logger;
        protected readonly ETagOption Options;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagMiddleware"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance used to logging.</param>
        /// <param name="options">The configuration IOptions.</param>
        /// <exception cref="ArgumentNullException">ILogger</exception>
        protected ETagCache(
            [NotNull] ILogger logger,
            [CanBeNull] IOptions<ETagOption> options)
            : this(logger, options?.Value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagMiddleware"/> class.
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> instance used to logging.</param>
        /// <param name="options">The configuration options.</param>
        /// <exception cref="ArgumentNullException">ILogger</exception>
        protected ETagCache(
            [NotNull] ILogger logger,
            [CanBeNull] ETagOption options)
        {
            if (options == null)
            {
                Options = new ETagOption();
            }
            else
            {
                Options = options;
                Options.BodyMaxLength = Options.BodyMaxLength == 0
                    ? ETagMiddlewareExtensions.DefaultBodyMaxLength
                    : Options.BodyMaxLength;
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Logger.LogDebug("The ETag algorithm is [{ETagAlgorithm}] and [{ETagValidator}] ETag Validator - MaxBodyLength=[{BodyMaxLength}].", Options.ETagAlgorithm.ToString(), Options.ETagValidator.ToString(), Options.BodyMaxLength);
        }

        protected void ManageEtag([NotNull] HttpContext context, [NotNull] Stream ms)
        {
            if (IsEtagSupportedOrNeeded(context))
            {
                StringValues ifNoneMatch = GetIfNoneMatch(context);
                // StringValues ifModifiedSince = GetIfModifiedSince(context);

                string etag = CreateETagAndAddToHeader(context, ms);

                CheckETagAndSetHttpStatusCode(context, ifNoneMatch.ToString(), etag);
            }
        }

        protected StringValues GetLastModified(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderNames.LastModified, out var lastModified))
            {
                Logger.LogInformation("Request has a Last-Modified::[{lastModified}] header.", lastModified.ToString());
            }

            return lastModified;
        }

        protected StringValues GetIfModifiedSince(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out var ifModifiedSince))
            {
                Logger.LogInformation("Request has an If-Modified-Since::[{ifModifiedSince}] header.", ifModifiedSince.ToString());
            }

            return ifModifiedSince;
        }

        protected StringValues GetIfNoneMatch(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatch))
            {
                Logger.LogInformation("Request has an If-None-Match::[{ifNoneMatch}] header.", ifNoneMatch.ToString());
            }

            return ifNoneMatch;
        }

        protected StringValues GetCacheControl(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderNames.CacheControl, out var cacheControl))
            {
                Logger.LogInformation("Request has an Cache-Control::[{cacheControl}] header.", cacheControl.ToString());
            }

            return cacheControl;
        }

        protected string CreateETagAndAddToHeader(HttpContext context, Stream ms)
        {
            string etag;
            if (ms.Length == 0)
            {
                etag = GetAndAddETagToHeader(context);
                Logger.LogDebug("Response has no body-content, fast etag is set to [{etag}].", etag);
            }
            else
            {
                etag = GetAndAddETagToHeader(context, ms);
            }

            return etag;
        }

        protected void CheckETagAndSetHttpStatusCode([NotNull] HttpContext context, [CanBeNull] string requestEtag, [NotNull] string etag)
        {
            if (!string.IsNullOrWhiteSpace(etag)
                && etag.Equals(requestEtag))
            {
                Logger.LogInformation("Response StatusCode is set to 304 (If-None-Match == ETag [{etag}]).", etag);
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                Logger.LogDebug("Response StatusCode is 304 (If-None-Match == ETag [{etag}]).", etag);
            }
        }

        protected bool IsNoCacheRequest(HttpContext context)
        {
            var cacheControl = GetCacheControl(context).ToString();
            if (string.IsNullOrWhiteSpace(cacheControl))
            {
                return false;
            }
            return !Regex.IsMatch(cacheControl, @"^((?!no-cache).)*$");
        }

        protected string GetResponseHash()
        {
            return ParseValidations(NoContentBodyHash);
        }

        protected string GetResponseHash([NotNull] Stream inputStream)
        {
            switch (Options.ETagAlgorithm)
            {
                case ETagAlgorithm.MD5:
                    using (var algo = MD5.Create())
                    {
                        return CreateHash(algo, inputStream);
                    }
                case ETagAlgorithm.SHA1:
                    using (var algo = SHA1.Create())
                    {
                        return CreateHash(algo, inputStream);
                    }
                case ETagAlgorithm.SHA265:
                    using (var algo = SHA256.Create())
                    {
                        return CreateHash(algo, inputStream);
                    }
                case ETagAlgorithm.SHA384:
                    using (var algo = SHA384.Create())
                    {
                        return CreateHash(algo, inputStream);
                    }
                case ETagAlgorithm.SHA521:
                    using (var algo = SHA512.Create())
                    {
                        return CreateHash(algo, inputStream);
                    }
            }
            throw new InvalidOperationException("ETagAlgorithm");
        }

        protected string ParseValidations([NotNull] string etag)
        {
            return Options.ETagValidator == ETagValidator.Strong
                   ? etag
                   : "W/" + etag;
        }

        protected string CreateHash([NotNull] HashAlgorithm hashAlgorithm, [NotNull] Stream inputStream)
        {
            inputStream.Position = 0;
            byte[] bytes = hashAlgorithm.ComputeHash(inputStream);
            Logger.LogDebug("Hash has a length of [{length}].", bytes.Length);
            return ParseValidations("\"" + WebEncoders.Base64UrlEncode(bytes) + "\"");
        }

        protected string GetAndAddETagToHeader([NotNull] HttpContext context)
        {
            return GetAndAddETagToHeader(context, null);
        }

        protected string GetAndAddETagToHeader([NotNull] HttpContext context, [CanBeNull] Stream ms)
        {
            var etag = ms == null
                ? GetResponseHash()
                : GetResponseHash(ms);

            AddEtagToHeader(context, etag);
            return etag;
        }

        protected void AddEtagToHeader([NotNull] HttpContext context, [NotNull] string etag)
        {
            Logger.LogInformation("Set to response {ETagValidator} ETag::[{etag}].", Options.ETagValidator.ToString(), etag);
            context.Response.Headers.Add(HeaderNames.ETag, etag);
            Logger.LogDebug("Response has {ETagValidator} ETag::[{etag}].", Options.ETagValidator.ToString(), etag);
        }

        protected bool IsMethodNotAllowed(string methods)
        {
            if (methods == HttpMethods.Get
                || methods == HttpMethods.Head)
            {
                return false;
            }
            return true;
        }

        protected bool IsEtagSupportedOrNeeded([NotNull] HttpContext context)
        {
            if (IsMethodNotAllowed(context.Request.Method))
            {
                Logger.LogDebug("The method [{Method}] does not support ETag.", context.Request.Method);
                return false;
            }

            if (IsNoCacheRequest(context))
            {
                Logger.LogDebug("The header [{CacheControl}] disables ETag for this request.", HeaderNames.CacheControl);
                return false;
            }

            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                Logger.LogDebug("The response status code [{StatusCode}] disables ETag for this request.", context.Response.StatusCode);
                return false;
            }

            if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
            {
                Logger.LogDebug("The response already contains an ETag header.");
                return false;
            }

            if (context.Response.Body.Length > Options.BodyMaxLength)
            {
                Logger.LogDebug("The response body length [{Length}] is longer than the BodyMaxLength [{BodyMaxLength}] configuration.", context.Response.Body.Length, Options.BodyMaxLength);
                return false;
            }

            return true;
        }
    }
}