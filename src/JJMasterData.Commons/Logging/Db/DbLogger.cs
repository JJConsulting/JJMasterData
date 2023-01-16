#nullable enable
using System;
using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.Db;

public class DbLogger : ILogger
{
    private readonly DbLoggerProvider _dbLoggerProvider;
    private bool _tableExists;
 
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
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var now = DateTime.Now;
        
        var values = new Hashtable
        {
            [_dbLoggerProvider.Options.CreatedColumnName] = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, now.Kind),
            [_dbLoggerProvider.Options.LevelColumnName] = (int)logLevel,
            [_dbLoggerProvider.Options.EventColumnName] = eventId.Name,
            [_dbLoggerProvider.Options.MessageColumnName] = GetMessage(eventId, formatter(state, exception), exception),
        };
        
        var element = DbLoggerElement.GetInstance(_dbLoggerProvider.Options);

        
        if (!_tableExists)
        {
            if (!_dbLoggerProvider.Repository.TableExists(_dbLoggerProvider.Options.TableName))
            {
                _dbLoggerProvider.Repository.CreateDataModel(element);
                _tableExists = true;
            }
        }
        
        _dbLoggerProvider.Repository.Insert(element, values);
    }

    private string GetMessage(EventId eventId, string formatterMessage, Exception? exception)
    {
        var message = new StringBuilder();

        message.AppendLine(eventId.Name);
        message.AppendLine(formatterMessage);

        if (exception != null)
        {
            message.AppendLine("Message:");
            message.AppendLine(exception.Message);
            message.AppendLine("Stacktrace:");
            message.AppendLine(exception.StackTrace);
            message.AppendLine("Source:");
            message.AppendLine(exception.Source);
        }

        return message.ToString();
    }
}