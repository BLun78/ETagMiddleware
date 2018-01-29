using System;
using BLun.ETagMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ETagMiddlewareTest
{
    [TestClass]
    public class ETagMiddlewareExtensionsTests
    {
        [TestMethod]
        public void UseETag_Without_Param_Ok()
        {
            // arange
            var app = Substitute.For<IApplicationBuilder>();

            // act
            app.UseETag();

            // Assert
            app.Received().Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
            Assert.IsNotNull(app);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "app")]
        public void UseETag_With_NullApp_Without_Param_Ok()
        {
            // arange
            IApplicationBuilder app = null;

            // act
            app.UseETag();

            // Assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        public void UseETag_With_Param_Ok()
        {
            // arange
            var app = Substitute.For<IApplicationBuilder>();

            // act
            app.UseETag(new ETagOption() { BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength });

            // Assert
            app.Received().Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
            Assert.IsNotNull(app);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "option")]
        public void UseETag_With_Null_Param_Ok()
        {
            // arange
            var app = Substitute.For<IApplicationBuilder>();

            // act
            app.UseETag(null);

            // Assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "app")]
        public void UseETag_With_NullApp_With_Param_Ok()
        {
            // arange
            IApplicationBuilder app = null;

            // act
            app.UseETag(new ETagOption() { BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength });

            // Assert
            Assert.Fail("No Exception");
        }
    }
}