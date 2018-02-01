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
            if (null == app.ApplicationServices.GetService(typeof(ETagMiddleware))) {
                throw new InvalidOperationException("No service for type 'BLun.ETagMiddleware.ETagMiddleware' has been registered. Add [services.AddETag()] in the method [public void ConfigureServices(IServiceCollection services)]");
            }

            return app.UseMiddleware<ETagMiddleware>();
        }

        public static IServiceCollection AddETag([NotNull] this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<ETagMiddleware>();

            return services;
        }

        public static IServiceCollection AddETag([NotNull] this IServiceCollection services, [NotNull] ETagOption configuration){
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            AddETag(services);

            return services.Configure<ETagOption>((ETagOption eTagOption) => {
                eTagOption.BodyMaxLength = configuration.BodyMaxLength;
                eTagOption.ETagValidator = configuration.ETagValidator;
                eTagOption.ETagAlgorithm = configuration.ETagAlgorithm;
            });
        }
    }
}