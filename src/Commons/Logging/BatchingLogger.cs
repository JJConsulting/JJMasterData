// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace JJMasterData.Commons.Logging;

internal sealed class BatchingLogger(BatchingLoggerProvider loggerProvider, string categoryName) : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return loggerProvider.IsEnabled;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var timestamp = DateTimeOffset.Now;
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = new StringBuilder();
        message.AppendLine(formatter(state, exception));
        if (exception is not null)
        {
            message.AppendLine("Exception:");
            message.AppendLine(exception.ToString());
            if (exception.StackTrace is not null)
            {
                message.AppendLine("StackTrace:");
                message.AppendLine(exception.StackTrace);
            }
            
            if (exception.Data.Contains("DataAccess Query"))
            {
                message.AppendLine("DataAccess Query:");
                message.AppendLine(exception.Data["DataAccess Query"].ToString());
            }
            if (exception.Data.Contains("DataAccess Parameters"))
            {
                message.AppendLine("DataAccess Parameters:");
                message.AppendLine(exception.Data["DataAccess Parameters"].ToString());
            }
        }
        loggerProvider.AddMessage(new LogMessage
        {
            Category = categoryName,
            Timestamp = timestamp,
            Message = message.ToString(),
            Event = eventId.Name ?? string.Empty,
            LogLevel = logLevel
        });
    }
}