using Microsoft.Extensions.Logging;

namespace JJMasterData.Web.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(
        Message = "Unhandled exception captured by ErrorController",
        Level = LogLevel.Critical)]
    internal static partial void LogUnhandledException(this ILogger logger, Exception exception);
}