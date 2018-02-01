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
    public class ETagTests
    {
        private sealed class TestETag : ETag
        {

            public TestETag(IOptions<ETagOption> options,
                            ILoggerFactory loggerFactory)
                : base(options,
                       loggerFactory.CreateLogger<TestETag>())
            {
            }

            public long BodyMaxLength => this._bodyMaxLength;
            public ILogger Logger => this._logger;
            public bool BaseIsEtagSupported(HttpContext context) => base.IsEtagSupportedOrNeeded(context);
            public void BaseAddEtagToHeader(HttpContext context, string etag) => base.AddEtagToHeader(context, etag);
            public string BaseClean(string etag) => base.Clean(etag);
        }

        internal ILoggerFactory CreateILoggerFactory()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger<ETagTests>>();

            loggerFactory.CreateLogger<ETagTests>().Returns(logger);

            return loggerFactory;
        }

        #region Ctor Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "options")]
        public void Create_And_Check_Ctor_Exceptions_Options_NOk()
        {
            // arange

            // act
            var etag = new TestETag(null, null);

            // assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_0_Then_DefaultBodyMaxLength_Ok()
        {
            // arange
            long length = 0;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETag(options, CreateILoggerFactory());

            // assert
            Assert.AreNotEqual(length, etag.BodyMaxLength);
            Assert.AreEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.BodyMaxLength);
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_10_Then_10_Ok()
        {
            // arange
            long length = 10;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETag(options, Substitute.For<ILoggerFactory>());

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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETag(options, null);

            // assert
            Assert.Fail("No Exception");
        }


        [TestMethod]
        public void Create_Check_Ctor_Created_Types_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETag(options, CreateILoggerFactory());

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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            context.Request.Returns(request);

            // act
            var result = etag.BaseIsEtagSupported(context);

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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(100);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            context.Request.Returns(request);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_HttpMethods_Post_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Post);
            context.Request.Returns(request);

            // act
            var result = etag.BaseIsEtagSupported(context);

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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(300);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            context.Request.Returns(request);

            // act
            var result = etag.BaseIsEtagSupported(context);

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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length + 10);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            context.Request.Returns(request);

            // act
            var result = etag.BaseIsEtagSupported(context);

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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(true);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            context.Request.Returns(request);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            Assert.IsFalse(result);
        }

        #endregion

        #region AddEtagToHeader Tests

        [TestMethod]
        public void AddEtagToHeader_Ok()
        {
            // arange
            var etagString = "HalloWeltEtag";
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            // act
            etag.BaseAddEtagToHeader(context, "HalloWeltEtag");

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.BodyMaxLength);
            context.Response.Headers.Received().Add(HeaderNames.ETag, etagString);
        }

        #endregion

        #region Clean

        [TestMethod]
        public void Clean_Ok()
        {
            // arange
            long length = 0;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, DefaultETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETag(options, Substitute.For<ILoggerFactory>());
            var testString = "He\"ll\"o";
            var expected = "Hello";

            // act
            var result = etag.BaseClean(testString);

            // assert
            Assert.AreEqual(expected, result);
        }

        #endregion

    }
}
