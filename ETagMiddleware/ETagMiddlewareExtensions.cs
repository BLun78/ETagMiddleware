using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace ETagMiddleware
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
        /// Enable Etag handshake
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UseETag([NotNull] this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            return UseETag(app, new ETagOption()
            {
                BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength
            });
        }
        
        /// <summary>
        /// Enable Etag handshake with given option
        /// </summary>
        /// <param name="app"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UseETag([NotNull] this IApplicationBuilder app,
                                                    [NotNull] ETagOption option)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (option == null) throw new ArgumentNullException(nameof(option));

            return app.UseMiddleware<global::ETagMiddleware.ETagMiddleware>(Options.Create(option));
        }
    }
}