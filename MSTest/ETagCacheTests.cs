using System;
using System.IO;
using BLun.ETagMiddleware;
using ETagMiddlewareTest.Common;
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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
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
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetLastModified_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
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

        #region GetIfModifiedSince

        [TestMethod]
        public void GetIfModifiedSince_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var name = HeaderNames.IfModifiedSince;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetIfModifiedSince(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetIfModifiedSince_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var name = HeaderNames.IfModifiedSince;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetIfModifiedSince(context);

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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
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
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
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

        #region GetCacheControl

        [TestMethod]
        public void GetCacheControl_Ok()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var name = HeaderNames.CacheControl;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetCacheControl(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetCacheControl_NOk()
        {
            // arange
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var name = HeaderNames.CacheControl;
            request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(request);

            // act
            var result = etag.BaseGetCacheControl(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }

        #endregion

        #region GetAndAddETagToHeader

        [TestMethod]
        public void GetAndAddETagToHeader_1_Pratameter_Ok()
        {
            // arange
            var expected = "\"z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg\"";
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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
            var result = etag.BaseGetAndAddETagToHeader(context);

            // assert
            Assert.IsNotNull(etag.Logger);
            Assert.AreEqual(length, etag.Options.BodyMaxLength);
            context.Response.Headers.Received().Add(HeaderNames.ETag,expected);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetAndAddETagToHeader_2_Pratameter_Ok()
        {
            // arange
            var expected = "\"z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg\"";
            long length = 100;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA521, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);
            var etag = new TestETagCache(options, LoggerMock.CreateILoggerFactory());

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

            using (var ms = new MemoryStream())
            {
                ms.Position = 0;

                // act
                var result = etag.BaseGetAndAddETagToHeader(context, ms);

                // assert
                Assert.IsNotNull(etag.Logger);
                Assert.AreEqual(length, etag.Options.BodyMaxLength);
                context.Response.Headers.Received().Add(HeaderNames.ETag, expected);
                Assert.AreEqual(expected, result);

            }
        }

        #endregion
    }
}
