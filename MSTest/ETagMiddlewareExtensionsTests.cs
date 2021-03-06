using System;
using BLun.ETagMiddleware;
using BLun.ETagMiddleware.Middleware;
using ETagMiddlewareTest.TestCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
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
            var loggerFactory = MockHelper.CreateILoggerFactory();
            var etagOption = Substitute.For<IOptions<ETagOption>>();
            var etagMiddleware = Substitute.For<ETagMiddleware>(loggerFactory, etagOption);
            var app = Substitute.For<IApplicationBuilder>();
            app.ApplicationServices.Returns(Substitute.For<IServiceProvider>());
            app.ApplicationServices.GetService(typeof(ETagMiddleware)).Returns(etagMiddleware);

            // act
            app.UseETag();

            // assert
            app.Received().Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
            Assert.IsNotNull(app);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), 
            "No service for type 'BLun.ETagMiddleware.ETagMiddleware' has been registered.Add[services.AddETag()] in the method[public void ConfigureServices(IServiceCollection services)]")]
        public void UseETag_Without_Param_Service_Null_NOk()
        {
            // arange
            var app = Substitute.For<IApplicationBuilder>();
            app.ApplicationServices.Returns(Substitute.For<IServiceProvider>());
            app.ApplicationServices.GetService(typeof(ETagMiddleware)).Returns((ETagMiddleware)null);

            // act
            app.UseETag();

            // assert
            Assert.Fail("No Exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "app")]
        public void UseETag_With_NullApp_Without_Param_NOk()
        {
            // arange
            IApplicationBuilder app = null;

            // act
            app.UseETag();

            // assert
            Assert.Fail("No Exception");
        }

    }
}