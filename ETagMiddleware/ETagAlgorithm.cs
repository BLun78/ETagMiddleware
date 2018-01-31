namespace BLun.ETagMiddleware
{
    public enum ETagAlgorithm 
    {
        StrongSHA1= 1, // default
        StrongSHA265 = 265,
        StrongSHA384 = 384,
        StrongSHA521 = 512,
        WeakMD5 = 5,
    }
}