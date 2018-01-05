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

namespace ETagMiddleware
{
    /// <summary>
    /// Enables ETag middleware for request
    /// </summary>
    public class ETagMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly long _bodyMaxLength;
        private readonly ILogger<ETagMiddleware> _logger;
        private readonly ETagOption _options;

        /// <summary>
        /// Create a new instance of the ETagMiddleware
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
        }

        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var originalStream = context.Response.Body;

            using (var ms = new MemoryStream())
            {
                context.Response.Body = ms;

                // Call the next delegate/middleware in the pipeline
                await this._next(context);

                ManageEtag(context, ms);

                ms.Position = 0;
                await ms.CopyToAsync(originalStream);
            }
        }

        private void ManageEtag(HttpContext context, MemoryStream ms)
        {
            if (IsEtagSupported(context))
            {
                StringValues requestEtag = string.Empty;
                if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out requestEtag))
                {
                    _logger.LogInformation($"Request has set If-None-Match::[{requestEtag.ToString()}] (ETag)");
                }

                var etag = GetAndAddETagToHeader(context, ms);
                if (requestEtag == etag)
                {
                    _logger.LogInformation($"Response is 304 (If-None-Match == ETag [{etag}])");
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                }
            }
        }

        private bool IsEtagSupported(HttpContext context)
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK)
                return false;

            if (context.Response.Body.Length > _bodyMaxLength)
                return false;

            if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
                return false;

            return true;
        }

        private string GetResponseHash(Stream inputStream)
        {
            using (var algo = SHA1.Create())
            {
                inputStream.Position = 0;
                byte[] bytes = algo.ComputeHash(inputStream);
                return $"\"{WebEncoders.Base64UrlEncode(bytes)}\"";
            }
        }

        private string GetAndAddETagToHeader(HttpContext context, MemoryStream ms)
        {
            var etag = GetResponseHash(ms);
            context.Response.Headers[HeaderNames.ETag] = etag;
            _logger.LogInformation($"Set to response strong ETag::[{etag}]");
            return etag;
        }
    }
}