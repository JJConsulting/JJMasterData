// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;

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

        loggerProvider.AddMessage(new LogMessage
        {
            Category = categoryName,
            Timestamp = timestamp,
            Message = formatter(state, exception),
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