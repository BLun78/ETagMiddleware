using System;
using System.IO;
using System.Threading.Tasks;
using BLun.ETagMiddleware.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware.Attribute
{
    internal class ETagCacheActionFilter : ETagCache, IAsyncActionFilter
    {
        public ETagCacheActionFilter(
            [NotNull] ILogger logger,
            [CanBeNull] IOptions<ETagOption> options) : base(logger, options)
        {
        }

        public ETagCacheActionFilter(
            [NotNull] ILogger logger,
            [CanBeNull] ETagOption options) : base(logger, options)
        {
        }
        
        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Stream originalStream = context.HttpContext.Response.Body;
            if (originalStream is MemoryStream)
            {
                // Call the next delegate/middleware in the pipeline
                await next();
                try
                {
                    ManageEtag(context.HttpContext, originalStream);
                }
                catch (Exception e)
                {
                    Logger.LogError($"In BLun.ETagAttribute is an error happend! >> Exception [{e}]", e);
                }
                finally
                {
                    if (context.HttpContext.Response.StatusCode == 200)
                    {
                        originalStream.Position = 0;
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    context.HttpContext.Response.Body = ms;

                    // Call the next delegate/middleware in the pipeline
                    await next();
                    try
                    {
                        ManageEtag(context.HttpContext, ms);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"In BLun.ETagAttribute is an error happend! >> Exception [{e}]", e);
                    }
                    finally
                    {
                        if (context.HttpContext.Response.StatusCode == 200)
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