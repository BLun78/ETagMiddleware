using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        /// <example>
        /// <code>
        /// // Add "app.UseETag();" to "Configure" method in Startup.cs
        /// public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        /// {
        ///     app.UseStaticFiles();
        /// 
        ///     // Add a Middleware for each Controller Request
        ///     // Atention: add app.UseETag after app.UseStaticFiles, the order is important
        ///     app.UseETag();
        /// 
        ///     app.UseMvc(routes =>
        ///     {
        ///         routes.MapRoute(
        ///             name: "default",
        ///             template: "{controller=Home}/{action=Index}/{id?}");
        ///     });
        /// }
        /// </code>
        /// </example>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">app</exception>
        /// <exception cref="InvalidOperationException">app.ApplicationServices.GetService(typeof(Middleware.ETagMiddleware))</exception>
        public static IApplicationBuilder UseETag([NotNull] this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (null == app.ApplicationServices.GetService(typeof(Middleware.ETagMiddleware))) {
                throw new InvalidOperationException("No service for type 'BLun.ETagMiddleware.ETagMiddleware' has been registered. Add [services.AddETag()] in the method [public void ConfigureServices(IServiceCollection services)]");
            }

            return app.UseMiddleware<Middleware.ETagMiddleware>();
        }

        /// <summary>
        /// Adds the ETag.
        /// </summary>
        /// <example>
        /// Default usage for AddETag()
        /// <code>
        /// // This method gets called by the runtime. Use this method to add services to the container.
        /// public void ConfigureServices(IServiceCollection services)
        /// {
        ///     services.AddMvc();
        /// 
        ///     // Required
        ///     // Add a Middleware for each Controller Request with
        ///     // algorithmus          = SHA1      = default
        ///     // etag validator       = Strong    = default
        ///     // body content length  = 40 * 1024 = default
        ///     services.AddETag();
        /// }
        /// </code>
        /// </example>
        /// <returns><see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/></returns>
        /// <param name="services">Services.</param>
        /// <exception cref="ArgumentNullException">services</exception>
        public static IServiceCollection AddETag([NotNull] this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddTransient<Middleware.ETagMiddleware>();

            return services;
        }

        /// <summary>
        /// Adds the ETag.
        /// </summary>
        /// <example>
        /// Own configuration for AddETag(configuration)
        /// <code>
        /// // This method gets called by the runtime. Use this method to add services to the container.
        /// public void ConfigureServices(IServiceCollection services)
        /// {
        ///     services.AddMvc();
        /// 
        ///     // Required
        ///     // Add ETagOption with own global configurations
        ///     services.AddETag(new ETagOption()
        ///     {
        ///         // algorithmus
        ///         // SHA1         = default
        ///         // SHA265
        ///         // SHA384
        ///         // SHA512
        ///         // MD5
        ///         ETagAlgorithm = ETagAlgorithm.SHA265,
        /// 
        ///         // etag validator
        ///         // Strong       = default
        ///         // Weak
        ///         ETagValidator = ETagValidator.Weak,
        /// 
        ///         // body content length
        ///         // 40 * 1024    = default
        ///         BodyMaxLength = 20 * 1024
        ///     });
        /// }
        /// </code>
        /// </example>
        /// <returns><see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/></returns>
        /// <param name="services">Services.</param>
        /// <param name="configuration">youre own Configuration.</param>
        /// <exception cref="ArgumentNullException">services and configuration</exception>
        public static IServiceCollection AddETag([NotNull] this IServiceCollection services, [NotNull] ETagOption configuration){
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            AddETag(services);

            return services.Configure((ETagOption eTagOption) => {
                eTagOption.BodyMaxLength = configuration.BodyMaxLength;
                eTagOption.ETagValidator = configuration.ETagValidator;
                eTagOption.ETagAlgorithm = configuration.ETagAlgorithm;
            });
        }
    }
}