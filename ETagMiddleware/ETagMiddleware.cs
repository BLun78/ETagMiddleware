using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware
{
    /// <summary>
    /// Enables ETag middleware for request
    /// </summary>
    public sealed class ETagMiddleware : ETag, IMiddleware
    {
        public ETagMiddleware(
            [NotNull] IOptions<ETagOption> options,
            [NotNull] ILoggerFactory loggerFactory) 
            : base(options, loggerFactory.CreateLogger<ETagMiddleware>())
        {
        }

        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            return base.BaseInvokeAsync(context, next);
        }
    }
}