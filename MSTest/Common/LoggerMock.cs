using System;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ETagMiddlewareTest.Common
{
    internal static class LoggerMock
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
    }
}