using System;
using System.IO;
using BLun.ETagMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ETagMiddlewareTest
{
    [TestClass]
    public class ETagMiddlewareTests
    {
        internal class TestETagMiddleware : ETagMiddleware
        {

            public TestETagMiddleware(RequestDelegate next,
                                      IOptions<ETagOption> options,
                                      ILoggerFactory loggerFactory)
                   : base(next, options, loggerFactory)
            {
            }

            public long BodyMaxLength => this._bodyMaxLength;
            public ILogger<ETagMiddleware> Logger => this._logger;
            public bool IsEtagSupported(HttpContext context) => base.IsEtagSupported(context);

        }

        internal ILoggerFactory CreateILoggerFactory()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger<ETagMiddleware>>();

            loggerFactory.CreateLogger<ETagMiddleware>().Returns(logger);

            return loggerFactory;
        }

        #region Ctor Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "next")]
        public void Create_And_Check_Ctor_Exceptions_Next_NOk()
        {
            // arange

            // act
            var etag = new ETagMiddleware(null, null, null);

            // assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "options")]
        public void Create_And_Check_Ctor_Exceptions_Options_NOk()
        {
            // arange

            // act
            var etag = new ETagMiddleware(Substitute.For<RequestDelegate>(), null, null);

            // assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_0_Then_DefaultBodyMaxLength_Ok()
        {
            // arange
            long length = 0;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(), options, Substitute.For<ILoggerFactory>());

            // assert
            Assert.AreNotEqual(length, etag.BodyMaxLength);
            Assert.AreEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.BodyMaxLength);
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_10_Then_10_Ok()
        {
            // arange
            long length = 10;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(), options, Substitute.For<ILoggerFactory>());

            // assert
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.AreNotEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.BodyMaxLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "loggerFactory")]
        public void Create_And_Check_Ctor_Exceptions_LoggerFactory_NOk()
        {
            // arange
            long length = 1;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new ETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          null);

            // assert
            Assert.Fail("No Exception");
        }


        [TestMethod]
        public void Create_Check_Ctor_Created_Types_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          CreateILoggerFactory());

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
        }

        #endregion

        #region IsEtagSupported Tests

        [TestMethod]
        public void IsEtagSupported_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            // act
            var result = etag.IsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEtagSupported_StatusCode_100_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(100);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            // act
            var result = etag.IsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_StatusCode_300_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(300);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            // act
            var result = etag.IsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_BodyMaxLength_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length + 10);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            // act
            var result = etag.IsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_Headers_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagMiddleware(Substitute.For<RequestDelegate>(),
                                          options,
                                          CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(true);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            // act
            var result = etag.IsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsFalse(result);
        }

        #endregion

    }
}
