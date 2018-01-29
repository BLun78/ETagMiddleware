using System;
using BLun.ETagMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ETagMiddlewareTest
{
    [TestClass]
    public class ETagMiddlewareTests
    {

        internal class TestETagMiddleware : ETagMiddleware{
            
            public TestETagMiddleware(RequestDelegate next,
                                      IOptions<ETagOption> options,
                                      ILoggerFactory loggerFactory)
                   : base(next,options,loggerFactory)
            {
            }

            public long BodyMaxLength => this._bodyMaxLength;


        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "next")]
        public void Create_And_Check_Ctor_Exceptions_Next_NOk()
        {
            // arange

            // act
            var etag = new ETagMiddleware(null, null, null);

            // Assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "options")]
        public void Create_And_Check_Ctor_Exceptions_Options_NOk()
        {
            // arange

            // act
            var etag = new ETagMiddleware(Substitute.For<RequestDelegate>(), null, null);

            // Assert
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

            // Assert
            Assert.AreNotEqual(length, etag.BodyMaxLength);
            Assert.AreEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength,etag.BodyMaxLength);
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

            // Assert
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
                                          
            // Assert
            Assert.Fail("No Exception");
        }
    }
}
