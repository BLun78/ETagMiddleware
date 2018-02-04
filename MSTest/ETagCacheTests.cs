using System;
using System.IO;
using BLun.ETagMiddleware;
using BLun.ETagMiddleware.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ETagMiddlewareTest
{
    [TestClass]
    public class ETagCacheTests
    {
        private sealed class TestETagCache : ETagCache
        {

            public TestETagCache(IOptions<ETagOption> options,
                            ILoggerFactory loggerFactory)
                : base(options,
                       loggerFactory.CreateLogger<TestETagCache>())
            {
            }

            public ETagOption Options => this._options;
            public ILogger Logger => this._logger;
            public void BaseManageEtag(HttpContext context, Stream ms) => base.ManageEtag(context, ms);
            public StringValues BaseGetLastModified(HttpContext context) => base.GetLastModified(context);
            public StringValues BaseGetIfModifiedSince(HttpContext context) => base.GetIfModifiedSince(context);
            public StringValues BaseGetIfNoneMatch(HttpContext context) => base.GetIfNoneMatch(context);
            public StringValues BaseGetCacheControl(HttpContext context) => base.GetCacheControl(context);
            public string BaseCreateETagAndAddToHeader(HttpContext context, Stream ms) 
                        => base.CreateETagAndAddToHeader(context, ms);
            public void BaseCheckETagAndSetHttpStatusCode(HttpContext context, string requestEtag, string etag)
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

        internal ILoggerFactory CreateILoggerFactory(params Action<ILogger>[] actions)
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var logger = Substitute.For<ILogger<ETagCacheTests>>();

            foreach(var action in actions){
                action?.Invoke(logger);
            }

            loggerFactory.CreateLogger<ETagCacheTests>().Returns(logger);

            return loggerFactory;
        }

        #region Ctor Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "options")]
        public void Create_And_Check_Ctor_Exceptions_Options_NOk()
        {
            // arange

            // act
            var etag = new TestETagCache(null, null);

            // assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_0_Then_DefaultBodyMaxLength_Ok()
        {
            // arange
            long length = 0;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagCache(options, CreateILoggerFactory());

            // assert
            Assert.AreNotEqual(length, etag.Options.BodyMaxLength);
            Assert.AreEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.Options.BodyMaxLength);
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_10_Then_10_Ok()
        {
            // arange
            long length = 10;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagCache(options, Substitute.For<ILoggerFactory>());

            // assert
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreNotEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.Options.BodyMaxLength);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "loggerFactory")]
        public void Create_And_Check_Ctor_Exceptions_LoggerFactory_NOk()
        {
            // arange
            long length = 1;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagCache(options, null);

            // assert
            Assert.Fail("No Exception");
        }


        [TestMethod]
        public void Create_Check_Ctor_Created_Types_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagCache(options, CreateILoggerFactory());

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
        }

        #endregion

        #region IsEtagSupported Tests

        [TestMethod]
        public void IsEtagSupported_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEtagSupported_StatusCode_100_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_HttpMethods_Post_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_StatusCode_300_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_BodyMaxLength_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_Headers_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };
            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
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
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

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
            etag.BaseAddEtagToHeader(context, "HalloWeltEtag");

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            context.Response.Headers.Received().Add(HeaderNames.ETag, etagString);
        }

        #endregion

        #region GetLastModified

        [TestMethod]
        public void GetLastModified_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            request.Headers.Returns(Substitute.For<IHeaderDictionary>());
            StringValues outValue;
            var name = HeaderNames.LastModified;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x => {
                if (((string)x[0]) == name) {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetLastModified(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetLastModified_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            request.Headers.Returns(Substitute.For<IHeaderDictionary>());
            StringValues outValue;
            var name = HeaderNames.LastModified;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x => {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetLastModified(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }

        #endregion

        #region GetIfNoneMatch

        [TestMethod]
        public void GetIfNoneMatch_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            request.Headers.Returns(Substitute.For<IHeaderDictionary>());
            StringValues outValue;
            var name = HeaderNames.IfNoneMatch;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x => {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetIfNoneMatch(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetIfNoneMatch_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, CreateILoggerFactory());

            var response = Substitute.For<HttpResponse>();
            response.Body.Returns(Substitute.For<Stream>());
            response.Body.Length.ReturnsForAnyArgs(length);
            response.StatusCode.Returns<int>(200);
            response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(false);

            var context = Substitute.For<HttpContext>();
            context.Response.Returns(response);

            var request = Substitute.For<HttpRequest>();
            request.Method.Returns(HttpMethods.Get);
            request.Headers.Returns(Substitute.For<IHeaderDictionary>());
            StringValues outValue;
            var name = HeaderNames.IfNoneMatch;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x => {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetIfNoneMatch(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }

        #endregion
    }
}
