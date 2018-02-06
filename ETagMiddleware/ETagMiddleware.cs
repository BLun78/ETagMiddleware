using System.Threading.Tasks;
using BLun.ETagMiddleware.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware
{
    /// <summary>
    /// Enables ETag middleware for request
    /// </summary>
    public class ETagMiddleware : IMiddleware
    {
        private readonly IMiddleware _etag;

        public ETagMiddleware(
            [NotNull] IOptions<ETagOption> options,
            [NotNull] ILoggerFactory loggerFactory) 
        {
            _etag = new ETagCacheMiddleware(options, loggerFactory.CreateLogger<ETagMiddleware>());
        }

        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return _etag.InvokeAsync(context, next);
        }
    }
}