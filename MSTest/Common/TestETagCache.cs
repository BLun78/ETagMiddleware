using System.IO;
using BLun.ETagMiddleware;
using BLun.ETagMiddleware.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace ETagMiddlewareTest.Common
{
    internal sealed class TestETagCache : ETagCache
    {
        public TestETagCache(
            ILoggerFactory loggerFactory,
            IOptions<ETagOption> options)
            : base(loggerFactory.CreateLogger<TestETagCache>(), options)
        {
        }

        public ETagOption BaseOptions => base.Options;
        public ILogger BaseLogger => base.Logger;
        public void BaseManageEtag(HttpContext context, Stream ms) => base.ManageEtag(context, ms);
        public StringValues BaseGetLastModified(HttpContext context) => base.GetLastModified(context);
        public StringValues BaseGetIfModifiedSince(HttpContext context) => base.GetIfModifiedSince(context);
        public StringValues BaseGetIfNoneMatch(HttpContext context) => base.GetIfNoneMatch(context);
        public StringValues BaseGetCacheControl(HttpContext context) => base.GetCacheControl(context);
        public string BaseCreateETagAndAddToHeader(HttpContext context, Stream ms)
            => base.CreateETagAndAddToHeader(context, ms);
        public void BaseCheckETagAndSetHttpStatusCode(HttpContext context, StringValues requestEtag, string etag)
            => base.CheckETagAndSetHttpStatusCode(context, requestEtag, etag);
        public bool BaseIsNoCacheRequest(HttpContext context) => base.IsNoCacheRequest(context);
        public string BaseGetResponseHash() => base.GetResponseHash();
        public string BaseGetResponseHash(Stream inputStream) => base.GetResponseHash(inputStream);
        public string BaseParseValidations(string etag) => base.ParseValidations(etag);
        public string BaseGetAndAddETagToHeader(HttpContext context) => base.GetAndAddETagToHeader(context);
        public string BaseGetAndAddETagToHeader(HttpContext context, Stream ms) => GetAndAddETagToHeader(context, ms);
        public void BaseAddEtagToHeader(HttpContext context, string etag) => base.AddEtagToHeader(context, etag);
        public bool BaseIsMethodNotAllowed(string methods) => base.IsMethodNotAllowed(methods);
        public bool BaseIsEtagSupported(HttpContext context) => base.IsEtagSupportedOrNeeded(context);
    }
}