using System;
using System.Threading.Tasks;
using BLun.ETagMiddleware.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ETagAttribute : Attribute, IAsyncActionFilter
    {
        private bool _isSetETagAlgorithm;
        private ETagAlgorithm eTagAlgorithm;
        public ETagAlgorithm ETagAlgorithm
        {
            get
            {
                return eTagAlgorithm;
            }

            set
            {
                _isSetETagAlgorithm = true;
                eTagAlgorithm = value;
            }
        }

        private bool _isSetETagValidator;
        private ETagValidator eTagValidator;
        public ETagValidator ETagValidator
        {
            get
            {
                return eTagValidator;
            }

            set
            {
                _isSetETagValidator = true;
                eTagValidator = value;
            }
        }

        private bool _isSetBodyMaxLength;
        private long bodyMaxLength;
        public long BodyMaxLength
        {
            get
            {
                return bodyMaxLength;
            }

            set
            {
                _isSetBodyMaxLength = true;
                bodyMaxLength = value;
            }
        }

        public Task OnActionExecutionAsync([NotNull] ActionExecutingContext context, [NotNull] ActionExecutionDelegate next)
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

            if (_isSetBodyMaxLength)
            {
                etagOption.BodyMaxLength = BodyMaxLength;
            }
            if (_isSetETagValidator)
            {
                etagOption.ETagValidator = ETagValidator;
            }
            if (_isSetETagAlgorithm)
            {
                etagOption.ETagAlgorithm = ETagAlgorithm;
            }

            IAsyncActionFilter etag = new ETagCacheActionFilter(etagOption, loggerFactory.CreateLogger<ETagAttribute>());

            return etag.OnActionExecutionAsync(context, next);
        }
    }
}
