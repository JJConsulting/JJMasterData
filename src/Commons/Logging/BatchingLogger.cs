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

    private void Log<TState>(DateTimeOffset timestamp, LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
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

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
        Func<TState, Exception, string> formatter)
    {
        Log(DateTimeOffset.Now, logLevel, eventId, state, exception, formatter);
    }
}