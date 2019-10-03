using System;
using Microsoft.Extensions.Logging;

namespace Routing.AspNetCore
{
    internal static class LoggerExtension
    {
        private static readonly Action<ILogger, Exception> ErrorWhileRoutingLoggerMessage =
            LoggerMessage.Define(
                eventId: new EventId(1, "ErrorWhileRouting"),
                logLevel: LogLevel.Error,
                formatString: "Error while routing.");

        private static readonly Action<ILogger, Exception> ErrorWhileConvertingResponseToContextLoggerMessage =
            LoggerMessage.Define(
                eventId: new EventId(1, "ErrorWhileConvertingResponseToContext"),
                logLevel: LogLevel.Error,
                formatString: "Error while converting response to context.");

        public static void ErrorWhileRouting(this ILogger logger, Exception exception)
        {
            ErrorWhileRoutingLoggerMessage(logger, exception);
        }

        public static void ErrorWhileConvertingResponseToContext(this ILogger logger, Exception exception)
        {
            ErrorWhileRoutingLoggerMessage(logger, exception);
        }
    }
}
