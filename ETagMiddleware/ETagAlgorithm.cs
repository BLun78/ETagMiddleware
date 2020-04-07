// ReSharper disable InconsistentNaming
namespace BLun.ETagMiddleware
{
    /// <summary>
    /// List of supported ETag algorithms for generating the ETag.
    /// </summary>
    public enum ETagAlgorithm
    {
        /// <summary>
        /// The Message-Digest Algorithm 5 - 128 bits.
        /// </summary>
        MD5 = 5,

        /// <summary>
        /// The Secure Hash Algorithm 1 - 160 bits.
        /// </summary>
        SHA1 = 1, // default

        /// <summary>
        /// The Secure Hash Algorithm 2 - 256 bits.
        /// </summary>
        SHA256 = 256,

        /// <summary>
        /// The Secure Hash Algorithm 2 - 384 bits.
        /// </summary>
        SHA384 = 384,

        /// <summary>
        /// The Secure Hash Algorithm 2 - 512 bits.
        /// </summary>
        SHA512 = 512,
    }
}