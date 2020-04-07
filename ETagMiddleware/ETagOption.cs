
namespace BLun.ETagMiddleware
{
    /// <summary>
    /// ETag option.
    /// </summary>
    /// <example>
    /// Default Configuration
    /// <code>
    /// var configuration = new ETagOption()
    /// {
    ///     // algorithmus
    ///     // MD5
    ///     // SHA1         = default
    ///     // SHA256
    ///     // SHA384
    ///     // SHA512
    ///     ETagAlgorithm = ETagAlgorithm.SHA1,
    /// 
    ///     // etag validator
    ///     // Strong       = default
    ///     // Weak
    ///     ETagValidator = ETagValidator.Strong,
    /// 
    ///     // body content length
    ///     // 40 * 1024    = default
    ///     BodyMaxLength = 40 * 1024
    /// };  
    /// </code>
    /// </example>
    public class ETagOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagOption"/> class.
        /// </summary>
        /// <example>
        /// Default Configuration
        /// <code>
        /// var configuration = new ETagOption()
        /// {
        ///     // algorithmus
        ///     // MD5
        ///     // SHA1         = default
        ///     // SHA256
        ///     // SHA384
        ///     // SHA512
        ///     ETagAlgorithm = ETagAlgorithm.SHA1,
        /// 
        ///     // etag validator
        ///     // Strong       = default
        ///     // Weak
        ///     ETagValidator = ETagValidator.Strong,
        /// 
        ///     // body content length
        ///     // 40 * 1024    = default
        ///     BodyMaxLength = 40 * 1024
        /// };  
        /// </code>
        /// </example>
        public ETagOption(){
            BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength;
            ETagAlgorithm = ETagAlgorithm.SHA1;
            ETagValidator = ETagValidator.Strong;
        }
            
        /// <summary>
        /// The max Body length for create Etag
        /// </summary>
        public long BodyMaxLength { get; set; }

        /// <summary>
        /// Gets or sets the ETag algorithm.
        /// </summary>
        /// <value>The ETag algorithm.</value>
        public ETagAlgorithm ETagAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the ETag validator.
        /// </summary>
        /// <value>The ETag validator.</value>
        public ETagValidator ETagValidator { get; set; }
    }
}