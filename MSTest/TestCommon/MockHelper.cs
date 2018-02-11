using System;
using System.IO;
using BLun.ETagMiddleware;
using ETagMiddlewareTest.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;

namespace ETagMiddlewareTest.TestCommon
{
    internal static class MockHelper
    {
        internal static ILoggerFactory CreateILoggerFactory(params Action<ILogger>[] actions)
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger<ETagCacheTests>>();

            foreach (var action in actions)
            {
                action?.Invoke(logger);
            }

            loggerFactory.CreateLogger<ETagCacheTests>().Returns(logger);

            return loggerFactory;
        }
        
        public static TestETagCache CreateETagCacheMock(out HttpContext context)
        {
            long length = 100;
            ETagOption etagOption = new ETagOption()
            {
                BodyMaxLength = length,
                ETagAlgorithm = ETagAlgorithm.SHA521,
                ETagValidator = ETagValidator.Strong
            };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, MockHelper.CreateILoggerFactory());

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            request.Headers.Returns(Substitute.For<IHeaderDictionary>());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            context = Substitute.For<HttpContext>();
            context.Response.Returns(response);
            context.Request.Returns(request);

            return etag;
        }
    }
}