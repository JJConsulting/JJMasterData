using System;
using System.IO;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Commons.Logging.File;

public class FileLogger : ILogger
{
    private readonly FileLoggerProvider _fileLoggerProvider;
 
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

        var values = new JObject
        {
            ["Created"] = DateTime.Now,
            ["LogLevel"] = logLevel.ToString(),
            ["EventId"] = eventId.Id,
            ["EventName"] = eventId.Name,
            ["Message"] = formatter(state, exception),
            ["ExceptionMessage"] = exception?.Message,
            ["ExceptionStackTrace"] = exception?.StackTrace,
            ["ExceptionSource"] = exception?.Source
        };
        
        var path = FileIO.ResolveFilePath(_fileLoggerProvider.Options.FileName);
        var directory = Path.GetDirectoryName(path);
        
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        using var writer = new StreamWriter(path, true);
        
        writer.Write(values);
    }
}