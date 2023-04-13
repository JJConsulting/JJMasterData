using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;


namespace JJMasterData.Commons.Logging.File;

public class FileLogger : ILogger
{
    private readonly FileLoggerProvider _fileLoggerProvider;
    private static readonly object locker = new();

    /// <summary>
    /// Creates a new instance of <see cref="FileLogger" />.
    /// </summary>
    public FileLogger(FileLoggerProvider fileLoggerProvider)
    {
        _fileLoggerProvider = fileLoggerProvider;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    /// <summary>
    /// Whether to log the entry.
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <summary>
    /// Used to log the entry.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
    /// <param name="eventId">The event's ID. An instance of <see cref="EventId"/>.</param>
    /// <param name="state">The event's state.</param>
    /// <param name="exception">The event's exception. An instance of <see cref="Exception" /></param>
    /// <param name="formatter">A delegate that formats </param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {

        if (!IsEnabled(logLevel))
            return;

        var path = FileIO.ResolveFilePath(_fileLoggerProvider.Options.CurrentValue.FileName);
        var directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        string record = GetLogRecord(logLevel, eventId.Name, formatter(state, exception), exception);

        lock (locker)
        {
            using var writer = new StreamWriter(path, true);
            writer.Write(record);
        }
    }

    private string GetLogRecord(LogLevel logLevel, string eventName, string message, Exception exception)
    {
        var log = new StringBuilder();

        log.AppendFormat("{0:yyyy-MM-dd HH:mm:ss+00:00} -", DateTime.Now);
        log.AppendFormat(" [{0}] ", logLevel);

        if (!string.IsNullOrWhiteSpace(eventName))
        {
            log.AppendFormat(" [{0}] ", eventName);
        }

        log.AppendFormat(" {0} ", message);

        if (exception != null)
        {
            log.AppendLine(exception.Message);
            log.AppendLine(exception.StackTrace);
            log.AppendFormat("Source: {0}", exception.Source);
        }

        log.AppendLine();

        return log.ToString();
    }
}