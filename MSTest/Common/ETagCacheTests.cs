using System;
using System.IO;
using BLun.ETagMiddleware;
using ETagMiddlewareTest.TestCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ETagMiddlewareTest.Common
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
            var etag = new TestETagCache(MockHelper.CreateILoggerFactory(), options);

            // assert
            Assert.AreNotEqual(length, etag.BaseOptions.BodyMaxLength);
            Assert.AreEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.BaseOptions.BodyMaxLength);
        }

        [TestMethod]
        public void Check_Option_BodyMaxLength_If_10_Then_10_Ok()
        {
            // arange
            long length = 10;
            ETagOption etagOption = new ETagOption() { BodyMaxLength = length, ETagAlgorithm = ETagAlgorithm.SHA1, ETagValidator = ETagValidator.Strong };

            IOptions<ETagOption> options = Options.Create(etagOption);

            // act
            var etag = new TestETagCache(Substitute.For<ILoggerFactory>(), options);

            // assert
            Assert.AreEqual(length, etag.BaseOptions.BodyMaxLength);
            Assert.AreNotEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, etag.BaseOptions.BodyMaxLength);
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
            var etag = new TestETagCache(null, options);

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
            var etag = new TestETagCache(MockHelper.CreateILoggerFactory(), options);

            // assert
            Assert.IsNotNull(etag.BaseLogger);
            Assert.AreEqual(length, etag.BaseOptions.BodyMaxLength);
        }

        #endregion

        #region IsEtagSupported Tests

        [TestMethod]
        public void IsEtagSupported_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEtagSupported_StatusCode_100_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            context.Response.StatusCode.Returns<int>(100);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_HttpMethods_Post_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            context.Request.Method.Returns(HttpMethods.Post);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_StatusCode_300_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            context.Response.StatusCode.Returns<int>(300);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_BodyMaxLength_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            long length = 100;
            context.Response.Body.Length.ReturnsForAnyArgs(length + 10);
           
            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.AreEqual(length, etag.BaseOptions.BodyMaxLength);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEtagSupported_Headers_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            context.Response.Headers.ContainsKey(HeaderNames.ETag).ReturnsForAnyArgs(true);

            // act
            var result = etag.BaseIsEtagSupported(context);

            // assert
            Assert.IsFalse(result);
        }

        #endregion

        #region AddEtagToHeader Tests

        [TestMethod]
        public void AddEtagToHeader_Ok()
        {
            // arange
            var etagString = "HalloWeltEtag";
            var etag = MockHelper.CreateETagCacheMock(out var context);

            // act
            etag.BaseAddEtagToHeader(context, "HalloWeltEtag");

            // assert
            context.Response.Headers.Received().Add(HeaderNames.ETag, etagString);
        }

        #endregion

        #region GetLastModified

        [TestMethod]
        public void GetLastModified_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            var name = HeaderNames.LastModified;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetLastModified(context);

            // assert
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetLastModified_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.LastModified;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetLastModified(context);

            // assert
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }

        #endregion

        #region GetIfModifiedSince

        [TestMethod]
        public void GetIfModifiedSince_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.IfModifiedSince;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetIfModifiedSince(context);

            // assert
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetIfModifiedSince_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.IfModifiedSince;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetIfModifiedSince(context);

            // assert
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }

        #endregion

        #region GetIfNoneMatch

        [TestMethod]
        public void GetIfNoneMatch_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.IfNoneMatch;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetIfNoneMatch(context);

            // assert
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetIfNoneMatch_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.IfNoneMatch;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetIfNoneMatch(context);

            // assert
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }

        #endregion

        #region GetCacheControl

        [TestMethod]
        public void GetCacheControl_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.CacheControl;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) == name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetCacheControl(context);

            // assert
            Assert.AreEqual("test_Value", result.ToString());
        }

        [TestMethod]
        public void GetCacheControl_NOk()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            StringValues outValue;
            const string name = HeaderNames.CacheControl;
            context.Request.Headers.TryGetValue(name, out outValue).ReturnsForAnyArgs(x =>
            {
                if (((string)x[0]) != name)
                {
                    x[1] = new StringValues("test_Value");
                    return true;
                }
                return false;
            });
            context.Request.Returns(context.Request);

            // act
            var result = etag.BaseGetCacheControl(context);

            // assert
            Assert.AreNotEqual("test_Value", result.ToString());
            Assert.AreEqual("", result.ToString());
        }


        #endregion

        #region GetAndAddETagToHeader

        [TestMethod]
        public void GetAndAddETagToHeader_1_Pratameter_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            var expected = "\"z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg\"";

            // act
            var result = etag.BaseGetAndAddETagToHeader(context);

            // assert
            context.Response.Headers.Received().Add(HeaderNames.ETag,expected);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetAndAddETagToHeader_2_Pratameter_Ok()
        {
            // arange
            var etag = MockHelper.CreateETagCacheMock(out var context);

            var expected = "\"z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg_SpIdNs6c5H0NE8XYXysP-DGNKHfuwvY7kxvUdBeoGlODJ6-SfaPg\"";

            using (var ms = new MemoryStream())
            {
                ms.Position = 0;

                // act
                var result = etag.BaseGetAndAddETagToHeader(context, ms);

                // assert
                context.Response.Headers.Received().Add(HeaderNames.ETag, expected);
                Assert.AreEqual(expected, result);
            }
        }

        #endregion
    }
}
