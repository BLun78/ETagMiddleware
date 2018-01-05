
namespace ETagMiddleware
{
    public class ETagOption
    {
        public ETagOption(){}
            
        /// <summary>
        /// max Body length for Etag
        /// </summary>
        public long BodyMaxLength { get; set; }
    }
}