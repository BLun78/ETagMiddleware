namespace BLun.ETagMiddleware
{
    /// <summary>
    /// ETag options.
    /// </summary>
    /// <example>
    /// Default Configuration
    /// <code>
    /// var configuration = new ETagOption()
    /// {
    ///     // algorithms
    ///     // SHA1         = default
    ///     // SHA265
    ///     // SHA384
    ///     // SHA512
    ///     // MD5
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
        ///     // algorithms
        ///     // SHA1         = default
        ///     // SHA265
        ///     // SHA384
        ///     // SHA512
        ///     // MD5
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
        public ETagOption()
        {
            BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength;
            ETagAlgorithm = ETagAlgorithm.SHA1;
            ETagValidator = ETagValidator.Strong;
        }

        /// <summary>
        /// Gets or sets the maximum body length to create an ETag for.
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