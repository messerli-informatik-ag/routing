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

        public static void ErrorSavingTheSession(this ILogger logger, Exception exception)
        {
            ErrorWhileRoutingLoggerMessage(logger, exception);
        }
    }
}
