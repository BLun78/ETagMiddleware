using System;
using System.Threading.Tasks;
using BLun.ETagMiddleware.Attribute;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLun.ETagMiddleware
{
    /// <summary>
    /// ETag ActionFilter attribute for MVC.
    /// AttributeTargets are CLass and Method
    /// </summary>
    /// <example>
    /// How to use:
    /// <code>
    /// // Can add on controller
    /// [ETag()]
    /// public class HomeController : Controller
    /// {
    /// 
    ///     public IActionResult Index()
    ///     {
    ///         return View();
    ///     }
    /// 
    ///     // Can add on methods
    ///     [ETag(ETagValidator = ETagValidator.Weak)]
    ///     public IActionResult About()
    ///     {
    ///         ViewData["Message"] = "Your application description page.";
    /// 
    ///         return View();
    ///     }
    /// 
    ///     [ETag(ETagValidator = ETagValidator.Strong, BodyMaxLength = 30 * 1024, ETagAlgorithm = ETagAlgorithm.SHA384)]
    ///     public IActionResult Contact()
    ///     {
    ///         ViewData["Message"] = "Your contact page.";
    /// 
    ///         return View();
    ///     }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ETagAttribute : System.Attribute, IAsyncActionFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagAttribute"/> class.
        /// </summary>
        /// <example>
        /// How to use:
        /// <code>
        /// // Can add on controller
        /// [ETag()]
        /// public class HomeController : Controller
        /// {
        /// 
        ///     public IActionResult Index()
        ///     {
        ///         return View();
        ///     }
        /// 
        ///     // Can add on methods
        ///     [ETag(ETagValidator = ETagValidator.Weak)]
        ///     public IActionResult About()
        ///     {
        ///         ViewData["Message"] = "Your application description page.";
        /// 
        ///         return View();
        ///     }
        /// 
        ///     [ETag(ETagValidator = ETagValidator.Strong, BodyMaxLength = 30 * 1024, ETagAlgorithm = ETagAlgorithm.SHA384)]
        ///     public IActionResult Contact()
        ///     {
        ///         ViewData["Message"] = "Your contact page.";
        /// 
        ///         return View();
        ///     }
        /// }
        /// </code>
        /// </example>
        // ReSharper disable once EmptyConstructor
        public ETagAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the ETag algorithm.
        /// </summary>
        /// <value>The ETag algorithm.</value>
        public ETagAlgorithm ETagAlgorithm
        {
            get => _eTagAlgorithm;
            set
            {
                _isSetETagAlgorithm = true;
                _eTagAlgorithm = value;
            }
        }
        private bool _isSetETagAlgorithm;
        private ETagAlgorithm _eTagAlgorithm;

        /// <summary>
        /// Gets or sets the ETag validator. (Strong ort Weak)
        /// </summary>
        /// <value>The ETag validator.</value>
        public ETagValidator ETagValidator
        {
            get => _eTagValidator;
            set
            {
                _isSetETagValidator = true;
                _eTagValidator = value;
            }
        }
        private bool _isSetETagValidator;
        private ETagValidator _eTagValidator;

        /// <summary>
        /// Gets or sets the max length of the body for generate the ETag.
        /// </summary>
        /// <value>The max length of the body.</value>
        public long BodyMaxLength
        {
            get => _bodyMaxLength;
            set
            {
                _isSetBodyMaxLength = true;
                _bodyMaxLength = value;
            }
        }
        private bool _isSetBodyMaxLength;
        private long _bodyMaxLength;

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">ILoggerFactory</exception>
        public Task OnActionExecutionAsync([NotNull] ActionExecutingContext context, [NotNull] ActionExecutionDelegate next)
        {
            var options = (IOptions<ETagOption>)context.HttpContext.RequestServices.GetService(typeof(IOptions<ETagOption>));
            var loggerFactory = (ILoggerFactory)context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
            if (loggerFactory == null) throw new InvalidOperationException("The ILoggerFactory is null! Register a service for ILoggerFactory!");

            var etagOption = new ETagOption();

            if (options?.Value != null)
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

            IAsyncActionFilter etag = new ETagCacheActionFilter(loggerFactory.CreateLogger<ETagAttribute>(), etagOption);

            return etag.OnActionExecutionAsync(context, next);
        }
    }
}
