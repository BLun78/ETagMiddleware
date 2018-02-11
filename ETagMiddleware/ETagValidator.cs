namespace BLun.ETagMiddleware
{
    /// <summary>
    /// List for ETag validator.
    /// Http-spec: https://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.3.3
    /// </summary>
    public enum ETagValidator
    {
        /// <summary>
        /// The strong validator.
        /// </summary>
        Strong,

        /// <summary>
        /// The weak validator.
        /// </summary>
        Weak
    }
}