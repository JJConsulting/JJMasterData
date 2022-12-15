using System;
using System.Collections;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.Db;

public class DbLogger : ILogger
{
    private readonly DbLoggerProvider _dbLoggerProvider;
 
    /// <summary>
    /// Creates a new instance of <see cref="DbLogger" />.
    /// </summary>
    public DbLogger(DbLoggerProvider dbLoggerProvider)
    {
        _dbLoggerProvider = dbLoggerProvider;
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
        {
            return;
        }

        var values = new Hashtable
        {
            ["Created"] = DateTime.Now,
            ["LogLevel"] = logLevel,
            ["EventId"] = eventId.Id,
            ["EventName"] = eventId.Name,
            ["Message"] = formatter(state, exception),
            ["ExceptionMessage"] = exception?.Message,
            ["ExceptionStackTrace"] = exception?.StackTrace,
            ["ExceptionSource"] = exception?.Source
        };

        string tableName = _dbLoggerProvider.Options.TableName;
        
        var element = DbLoggerElement.GetInstance(tableName);
        
        if (!_dbLoggerProvider.Repository.TableExists(tableName))
            _dbLoggerProvider.Repository.CreateDataModel(element);
        
        _dbLoggerProvider.Repository.Insert(element, values);
    }
}