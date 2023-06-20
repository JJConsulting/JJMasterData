#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.Db;

public class DbLogger : ILogger
{
    private bool tableExists;
    
    private readonly string _name;
    private readonly BlockingCollection<LogMessage> _queue;
    private readonly IEntityRepository _entityRepository;
    private readonly Func<DbLoggerOptions> _getCurrentOptions;

    /// <summary>
    /// Creates a new instance of <see cref="DbLogger" />.
    /// </summary>
    public DbLogger(
        string name, 
        BlockingCollection<LogMessage> queue,
        IEntityRepository entityRepository,
        Func<DbLoggerOptions> getCurrentOptions
    )
    {
        _name = name;
        _queue = queue;
        _entityRepository = entityRepository;
        _getCurrentOptions = getCurrentOptions;
        Task.Factory.StartNew(LogAtDatabase, TaskCreationOptions.LongRunning);
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
        
        var message = new LogMessage(logLevel, eventId, state!, exception, (s, e) => formatter((TState)s, e));
        _queue.Add(message);
    }
    
    private void LogAtDatabase()
    {
        foreach (var message in _queue.GetConsumingEnumerable())
        {
            var now = DateTime.Now;

            var options = _getCurrentOptions();

            var values = new Hashtable
            {
                [options.CreatedColumnName] = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, now.Kind),
                [options.LevelColumnName] = (int)message.LogLevel,
                [options.EventColumnName] = message.EventId.Name,
                [options.MessageColumnName] = GetMessage(message.EventId, message.Formatter(message.State, message.Exception), message.Exception),
            };
        
            var element = DbLoggerElement.GetInstance(options);
            
            if (!tableExists)
            {
                if (!_entityRepository.TableExists(options.TableName))
                {
                    _entityRepository.CreateDataModel(element);
                }

                tableExists = true;
            }
        
            _entityRepository.Insert(element, values);
        }
    }

    private static string GetMessage(EventId eventId, string formatterMessage, Exception? exception)
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