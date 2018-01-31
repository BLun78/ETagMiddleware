using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace BLun.ETagMiddleware
{
    /// <summary>
    /// Enables ETag middleware for request
    /// </summary>
    public class ETagMiddleware
    {
        protected const string NoContentBodyHash = "z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg";
        protected readonly RequestDelegate _next;
        protected readonly long _bodyMaxLength;
        protected readonly ILogger<ETagMiddleware> _logger;
        protected readonly ETagOption _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="loggerFactory">An <see cref="ILoggerFactory"/> instance used to create loggers.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ETagMiddleware([NotNull] RequestDelegate next,
            [NotNull] IOptions<ETagOption> options,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;
            _bodyMaxLength = _options.BodyMaxLength > 0
                ? _options.BodyMaxLength
                : ETagMiddlewareExtensions.DefaultBodyMaxLength;

            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<ETagMiddleware>();
            _logger.LogDebug($"The Etag algorithm is []{_options.ETagAlgorithm.ToString()}");
        }

        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke([NotNull] HttpContext context)
        {
            var originalStream = context.Response.Body;

            using (var ms = new MemoryStream())
            {
                context.Response.Body = ms;

                // Call the next delegate/middleware in the pipeline
                await this._next(context);
                try
                {
                    ManageEtag(context, ms);
                }
                catch (Exception e)
                {
                    _logger.LogError($"In BLun.ETagMiddleware is an error happend! >> Exception [{e.Message}]", e);
                }
                finally
                {
                    ms.Position = 0;
                    await ms.CopyToAsync(originalStream);
                }
            }
        }

        protected void ManageEtag([NotNull] HttpContext context, [NotNull] MemoryStream ms)
        {
            if (IsEtagSupportedOrNeeded(context))
            {
                StringValues requestEtag = string.Empty;
                if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out requestEtag))
                {
                    _logger.LogInformation($"Request has an If-None-Match::[{requestEtag.ToString()}] header");
                }

                var etag = string.Empty;
                if (context.Response.Body.Length == 0)
                {
                    etag = NoContentBodyHash;
                    AddEtagToHeader(context, etag);
                    _logger.LogDebug($"Response has no body-content, fast etag is set to [{etag}]");
                }
                else
                {
                    etag = GetAndAddETagToHeader(context, ms);
                }

                CheckETagAndSetHttpStatusCode(context, requestEtag, etag);
            }
        }

        protected void CheckETagAndSetHttpStatusCode(HttpContext context, StringValues requestEtag, string etag)
        {
            if (!string.IsNullOrWhiteSpace(etag)
                && Clean(requestEtag) == Clean(etag))
            {
                _logger.LogInformation($"Response StatusCode is set to 304 (If-None-Match == ETag [{etag}])");
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                _logger.LogDebug($"Response StatusCode is 304 (If-None-Match == ETag [{etag}])");
            }
        }

        protected string Clean([NotNull]string etag){
            return etag.Replace(@"""","");
        }

        protected string GetResponseHash([NotNull] Stream inputStream)
        {
            switch (_options.ETagAlgorithm)
            { 
                case ETagAlgorithm.WeakMD5:
                    using (var algo = MD5.Create())
                    {
                        return $"W/{CreateHash(algo, inputStream)}";
                    }
                case ETagAlgorithm.StrongSHA1:
                    using (var algo = SHA1.Create())
                    {
                        return CreateHash(algo, inputStream); 
                    }
                case ETagAlgorithm.StrongSHA265:
                    using (var algo = SHA256.Create())
                    {
                        return CreateHash(algo, inputStream); 
                    }
                case ETagAlgorithm.StrongSHA384:
                    using (var algo = SHA384.Create())
                    {
                        return CreateHash(algo, inputStream); 
                    }
                case ETagAlgorithm.StrongSHA521:
                    using (var algo = SHA512.Create())
                    { 
                        return CreateHash(algo, inputStream); 
                    }
            }
            throw new InvalidOperationException("ETagAlgorithm");
        }

        protected string CreateHash([NotNull] HashAlgorithm hashAlgorithm, [NotNull] Stream inputStream)
        {
            inputStream.Position = 0;
            byte[] bytes = hashAlgorithm.ComputeHash(inputStream);
            return $"\"{WebEncoders.Base64UrlEncode(bytes)}\"";
        }

        protected string GetAndAddETagToHeader([NotNull] HttpContext context, [NotNull] MemoryStream ms)
        {
            var etag = GetResponseHash(ms);
            AddEtagToHeader(context, etag);
            return etag;
        }

        protected void AddEtagToHeader([NotNull] HttpContext context, [NotNull] string etag)
        {
            _logger.LogInformation($"Set to response strong ETag::[{etag}]");
            context.Response.Headers.Add(HeaderNames.ETag, etag);
            _logger.LogDebug($"Response has strong ETag::[{etag}]");
        }

        protected bool IsEtagSupportedOrNeeded([NotNull] HttpContext context)
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                _logger.LogDebug($"The HttpStatusCode is not 200! HttpStatusCode=[{context.Response.StatusCode.ToString()}]");
                return false;
            }

            if (context.Response.Body.Length > _bodyMaxLength)
            {
                _logger.LogDebug($"The Body.Length=[{context.Response.Body.Length}] is bigger then the BodyMaxLength=[{_bodyMaxLength}] configuration.");
                return false;
            }

            if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
            {
                _logger.LogDebug("The respons contains an ETag header.");
                return false;
            }

            return true;
        }
    }
}