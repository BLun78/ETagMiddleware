
namespace BLun.ETagMiddleware
{
    /// <summary>
    /// ETag option.
    /// </summary>
    public class ETagOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BLun.ETagMiddleware.ETagOption"/> class.
        /// </summary>
        public ETagOption(){}
            
        /// <summary>
        /// max Body length for Etag
        /// </summary>
        public long BodyMaxLength { get; set; }

        public ETagAlgorithm ETagAlgorithm { get; set; }
    }
}