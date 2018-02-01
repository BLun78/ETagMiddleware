using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ETagAttribute : Attribute, IAsyncActionFilter
    {
        private class ETagActionFilter : ETag
        {
            public ETagActionFilter(
                [CanBeNull] ETagOption options,
                [NotNull] ILogger logger)
                : base(options, logger)
            {
            }
        }

        private bool isSetETagAlgorithm;
        private ETagAlgorithm eTagAlgorithm;
        public ETagAlgorithm ETagAlgorithm
        {
            get
            {
                return eTagAlgorithm;
            }

            set
            {
                isSetETagAlgorithm = true;
                eTagAlgorithm = value;
            }
        }

        private bool isSetETagValidator;
        private ETagValidator eTagValidator;
        public ETagValidator ETagValidator
        {
            get
            {
                return eTagValidator;
            }

            set
            {
                isSetETagValidator = true;
                eTagValidator = value;
            }
        }

        private bool isSetBodyMaxLength;
        private long bodyMaxLength;
        public long BodyMaxLength
        {
            get
            {
                return bodyMaxLength;
            }

            set
            {
                isSetBodyMaxLength = true;
                bodyMaxLength = value;
            }
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var options = (IOptions<ETagOption>)context.HttpContext.RequestServices.GetService(typeof(IOptions<ETagOption>));
            var loggerFactory = (ILoggerFactory)context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
            if (loggerFactory == null) throw new InvalidOperationException("The ILoggerFactory is null! Register a service for ILoggerFactory!");

            var etagOption = new ETagOption();

            if (options != null && options.Value != null)
            {
                etagOption.BodyMaxLength = options.Value.BodyMaxLength;
                etagOption.ETagValidator = options.Value.ETagValidator;
                etagOption.ETagAlgorithm = options.Value.ETagAlgorithm;
            }

            if (isSetBodyMaxLength)
            {
                etagOption.BodyMaxLength = BodyMaxLength;
            }
            if (isSetETagValidator)
            {
                etagOption.ETagValidator = ETagValidator;
            }
            if (isSetETagAlgorithm)
            {
                etagOption.ETagAlgorithm = ETagAlgorithm;
            }

            var etag = new ETagActionFilter(etagOption, loggerFactory.CreateLogger<ETagAttribute>());

            return etag.OnActionExecutionAsync(context, next);
        }
    }
}
