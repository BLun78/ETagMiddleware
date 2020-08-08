using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

#if NETSTANDARD1_3
    using IMiddleware = BLun.ETagMiddleware.Middleware.Microsoft.AspNetCore.Http.IMiddleware;
#endif

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
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="options">Options.</param>
        public ETagMiddleware(
            [NotNull] ILoggerFactory loggerFactory,
            [CanBeNull] IOptions<ETagOption> options)
        {
            _etag = new ETagCacheMiddleware(loggerFactory.CreateLogger<ETagMiddleware>(), options);
        }

        /// <inheritdoc />
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return _etag.InvokeAsync(context, next);
        }
    }
}