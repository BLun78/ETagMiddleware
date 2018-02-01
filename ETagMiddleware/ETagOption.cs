
namespace BLun.ETagMiddleware
{
    /// <summary>
    /// ETag option.
    /// </summary>
    public sealed class ETagOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagOption"/> class.
        /// </summary>
        public ETagOption(){
            this.BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength;
            this.ETagAlgorithm = ETagAlgorithm.SHA1;
            this.ETagValidator = ETagValidator.Strong;
        }
            
        /// <summary>
        /// max Body length for Etag
        /// </summary>
        public long BodyMaxLength { get; set; }

        public ETagAlgorithm ETagAlgorithm { get; set; }

        public ETagValidator ETagValidator { get; set; }
    }
}