using System;
using JJMasterData.Commons.Data;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

public static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Message = "Error while executing DataAccessCommand. Command: {command}",
        Level = LogLevel.Critical)]
    public static partial void LogDataAccessCommandException(this ILogger logger, Exception exception, DataAccessCommand command);
}