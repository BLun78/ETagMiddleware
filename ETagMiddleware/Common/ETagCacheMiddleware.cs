using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware.Common
{
    internal class ETagCacheMiddleware : ETagCache, IMiddleware
    {
        public ETagCacheMiddleware([NotNull] IOptions<ETagOption> options, [NotNull] ILogger logger) : base(options, logger)
        {
        }

        public ETagCacheMiddleware([CanBeNull] ETagOption options, [NotNull] ILogger logger) : base(options, logger)
        {
        }
        
        /// <summary>
        /// Processes a request to do the ETag handshake
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
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
            else
            {
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
    }
}