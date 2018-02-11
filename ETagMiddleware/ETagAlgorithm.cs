namespace BLun.ETagMiddleware
{
    /// <summary>
    /// List of supported ETag algorithmen for genrate the ETag.
    /// </summary>
    public enum ETagAlgorithm 
    {
        /// <summary>
        /// The Secure Hash Algorithm 1.
        /// </summary>
        SHA1 = 1, // default

        /// <summary>
        /// The Secure Hash Algorithm 2 - 265 Bit.
        /// </summary>
        SHA265 = 265,

        /// <summary>
        /// The Secure Hash Algorithm 2 - 384 Bit.
        /// </summary>
        SHA384 = 384,

        /// <summary>
        /// The Secure Hash Algorithm 2 - 512 Bit.
        /// </summary>
        SHA521 = 512,

        /// <summary>
        /// The Message-Digest Algorithm 5 - 128 Bit - 32 chars in hexadezimal.
        /// </summary>
        MD5 = 5,
    }
}