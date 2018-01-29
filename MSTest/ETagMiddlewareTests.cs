using System;
using BLun.ETagMiddleware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ETagMiddlewareTest
{
    [TestClass]
    public class ETagMiddlewareTests
    {
        [TestMethod]
        public void Create_And_Check_Value()
        {
            // arange
            var option = new ETagOption();

            // act
            option.BodyMaxLength = 1;

            // Assert
            Assert.IsNotNull(option);
            Assert.AreEqual(1, option.BodyMaxLength);
        }

        [TestMethod]
        public void Create_And_Check_Value2()
        {
            // arange
            var option = new ETagOption();

            // act
            option.BodyMaxLength = ETagMiddlewareExtensions.DefaultBodyMaxLength;

            // Assert
            Assert.IsNotNull(option);
            Assert.AreEqual(ETagMiddlewareExtensions.DefaultBodyMaxLength, option.BodyMaxLength);
        }
    }
}
