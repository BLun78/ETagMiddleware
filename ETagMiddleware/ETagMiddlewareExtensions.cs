using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware
{
    /// <summary>
    /// Extension methods for the ETagMiddleware
    /// </summary>
    public static class ETagMiddlewareExtensions
    {
        /// <summary>
        /// Default max Body length for Etag
        /// </summary>
        public static long DefaultBodyMaxLength => 40 * 1024;

        /// <summary>
        /// Enable Etag handshake with given option
        /// </summary>
        /// <param name="app"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UseETag([NotNull] this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<global::BLun.ETagMiddleware.ETagMiddleware>();
        }

        public static IServiceCollection AddETag([NotNull] this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<ETagMiddleware>();

            return services;
        }
    }
}