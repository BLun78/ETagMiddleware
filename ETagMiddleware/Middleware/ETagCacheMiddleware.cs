﻿using System;
using System.IO;
using System.Threading.Tasks;
using BLun.ETagMiddleware.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware.Middleware
{
    internal class ETagCacheMiddleware : ETagCache, IMiddleware
    {
        public ETagCacheMiddleware(
            [NotNull] ILogger logger,
            [CanBeNull] IOptions<ETagOption> options) : base(logger, options)
        {
        }

        public ETagCacheMiddleware(
            [NotNull] ILogger logger,
            [CanBeNull] ETagOption options) : base(logger, options)
        {
        }

        /// <inheritdoc />
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
                    Logger.LogError(e, "An exception occured in BLun.ETagMiddleware! >> Exception [{e}]", e);
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
                        Logger.LogError(e, "An exception occured in BLun.ETagMiddleware! >> Exception [{e}]", e);
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