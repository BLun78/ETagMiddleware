using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
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
    public abstract class ETag
    {
        protected const string NoContentBodyHash = "\"z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg\"";
        protected readonly long _bodyMaxLength;
        protected readonly ILogger _logger;
        protected readonly ETagOption _options;

        public ETag(
            [NotNull] IOptions<ETagOption> options,
            [NotNull] ILogger logger) 
            : this(options?.Value, logger)
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagMiddleware"/> class.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">An <see cref="ILogger"/> instance used to logging.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ETag(
            [NotNull] ETagOption options,
            [NotNull] ILogger logger)
        {
            if (options == null){
                _options = new ETagOption();

            } else {
                _options = options;
            }
            _bodyMaxLength = _options.BodyMaxLength > 0
                   ? _options.BodyMaxLength
                   : ETagMiddlewareExtensions.DefaultBodyMaxLength;
            
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;
            _logger.LogDebug($"The Etag algorithm is [{_options.ETagAlgorithm.ToString()}] and {_options.DefaultETagValidator.ToString()} ETag");
        }

        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected async Task BaseInvokeAsync(HttpContext context, RequestDelegate next)
        {
            Stream originalStream = context.Response.Body;
            if (originalStream is MemoryStream)
            {
                // Call the next delegate/middleware in the pipeline
                await next(context);
                try
                {
                    ManageEtag(context, originalStream);
                }
                catch (Exception e)
                {
                    _logger.LogError($"In BLun.ETagMiddleware is an error happend! >> Exception [{e}]", e);
                }
                finally
                {
                    if (context.Response.StatusCode == 200)
                    {
                        originalStream.Position = 0;
                    }
                }
            }
            else {
                using (var ms = new MemoryStream())
                {
                    context.Response.Body = ms;

                    // Call the next delegate/middleware in the pipeline
                    await next(context);
                    try
                    {
                        ManageEtag(context, ms);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"In BLun.ETagMiddleware is an error happend! >> Exception [{e}]", e);
                    }
                    finally
                    {
                        if (context.Response.StatusCode == 200)
                        {
                            ms.Position = 0;
                            await ms.CopyToAsync(originalStream);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Stream originalStream = context.HttpContext.Response.Body;
            if (originalStream is MemoryStream)
            {
                // Call the next delegate/middleware in the pipeline
                await next();
                try
                {
                    ManageEtag(context.HttpContext, originalStream);
                }
                catch (Exception e)
                {
                    _logger.LogError($"In BLun.ETagAttribute is an error happend! >> Exception [{e}]", e);
                }
                finally
                {
                    if (context.HttpContext.Response.StatusCode == 200)
                    {
                        originalStream.Position = 0;
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    context.HttpContext.Response.Body = ms;

                    // Call the next delegate/middleware in the pipeline
                    await next();
                    try
                    {
                        ManageEtag(context.HttpContext, ms);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"In BLun.ETagAttribute is an error happend! >> Exception [{e}]", e);
                    }
                    finally
                    {
                        if (context.HttpContext.Response.StatusCode == 200)
                        {
                            ms.Position = 0;
                            await ms.CopyToAsync(originalStream);
                        }
                    }
                }
            }
        }

        protected void ManageEtag([NotNull] HttpContext context, [NotNull] Stream ms)
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
                    etag = GetAndAddETagToHeader(context);
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

        protected string Clean([NotNull]string etag)
        {
            return etag.Replace(@"""", "");
        }

        protected string GetResponseHash([NotNull] HttpContext context)
        {
            return ParseValidations(context, NoContentBodyHash);
        }

        protected string GetResponseHash([NotNull] HttpContext context, [NotNull] Stream inputStream)
        {
            switch (_options.ETagAlgorithm)
            {
                case ETagAlgorithm.MD5:
                    using (var algo = MD5.Create())
                    {
                        return CreateHash(context, algo, inputStream);
                    }
                case ETagAlgorithm.SHA1:
                    using (var algo = SHA1.Create())
                    {
                        return CreateHash(context, algo, inputStream);
                    }
                case ETagAlgorithm.SHA265:
                    using (var algo = SHA256.Create())
                    {
                        return CreateHash(context, algo, inputStream);
                    }
                case ETagAlgorithm.SHA384:
                    using (var algo = SHA384.Create())
                    {
                        return CreateHash(context, algo, inputStream);
                    }
                case ETagAlgorithm.SHA521:
                    using (var algo = SHA512.Create())
                    {
                        return CreateHash(context, algo, inputStream);
                    }
            }
            throw new InvalidOperationException("ETagAlgorithm");
        }

        protected string ParseValidations([NotNull] HttpContext context, [NotNull] string etag)
        {


            if (_options.DefaultETagValidator == ETagValidator.Strong)
            {
                return etag;
            }
            else
            {
                return $"W/{etag}";
            }
        }

        protected string CreateHash([NotNull] HttpContext context, [NotNull] HashAlgorithm hashAlgorithm, [NotNull] Stream inputStream)
        {
            inputStream.Position = 0;
            byte[] bytes = hashAlgorithm.ComputeHash(inputStream);

            return ParseValidations(context, $"\"{WebEncoders.Base64UrlEncode(bytes)}\"");
        }

        protected string GetAndAddETagToHeader([NotNull] HttpContext context)
        {
            return GetAndAddETagToHeader(context, null);
        }

        protected string GetAndAddETagToHeader([NotNull] HttpContext context, [CanBeNull] Stream ms)
        {
            var etag = string.Empty;
            if (ms == null)
            {
                etag = GetResponseHash(context);
            }
            else
            {
                etag = GetResponseHash(context, ms);
            }

            AddEtagToHeader(context, etag);
            return etag;
        }

        protected void AddEtagToHeader([NotNull] HttpContext context, [NotNull] string etag)
        {
            _logger.LogInformation($"Set to response {_options.DefaultETagValidator.ToString()} ETag::[{etag}]");
            context.Response.Headers.Add(HeaderNames.ETag, etag);
            _logger.LogDebug($"Response has {_options.DefaultETagValidator.ToString()} ETag::[{etag}]");
        }

        protected bool IsEtagSupportedOrNeeded([NotNull] HttpContext context)
        {
            if (context.Request.Method != HttpMethods.Get)
            {
                _logger.LogDebug($"The HttpMethode [{context.Request.Method}] is not suportet for ETag.");
                return false;
            }

            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                _logger.LogDebug($"The HttpStatusCode is not 200! HttpStatusCode=[{context.Response.StatusCode.ToString()}]");
                return false;
            }

            if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
            {
                _logger.LogDebug("The respons contains an ETag header.");
                return false;
            }

            if (context.Response.Body.Length > _bodyMaxLength)
            {
                _logger.LogDebug($"The Body.Length=[{context.Response.Body.Length}] is bigger then the BodyMaxLength=[{_bodyMaxLength}] configuration.");
                return false;
            }

            return true;
        }
    }
}