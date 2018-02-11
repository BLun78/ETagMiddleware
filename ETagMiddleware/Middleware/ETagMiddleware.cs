using System.Threading.Tasks;
using BLun.ETagMiddleware.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware.Middleware
{
    /// <summary>
    /// Enables ETag middleware for request
    /// </summary>
    internal class ETagMiddleware : IMiddleware
    {
        private readonly IMiddleware _etag;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.Middleware.ETagMiddleware"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public ETagMiddleware(
            [NotNull] IOptions<ETagOption> options,
            [NotNull] ILoggerFactory loggerFactory) 
        {
            _etag = new ETagCacheMiddleware(options, loggerFactory.CreateLogger<ETagMiddleware>());
        }

        /// <inheritdoc />
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return _etag.InvokeAsync(context, next);
        }
    }
}