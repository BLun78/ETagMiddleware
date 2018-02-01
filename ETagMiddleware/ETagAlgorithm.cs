namespace BLun.ETagMiddleware
{
    public enum ETagAlgorithm 
    {
        SHA1= 1, // default
        SHA265 = 265,
        SHA384 = 384,
        SHA521 = 512,
        MD5 = 5,
    }
}