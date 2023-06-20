using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias("Database")]
public class DbLoggerProvider : ILoggerProvider
{
    private IOptionsMonitor<DbLoggerOptions> Options { get; }
    private IEntityRepository Repository { get; }
    private bool TableExists { get; set; }
    
    private readonly BlockingCollection<LogMessage> _queue;
    public DbLoggerProvider(IOptionsMonitor<DbLoggerOptions> options, IEntityRepository entityRepository)
    {
        Repository = entityRepository;
        Options = options;
        
        _queue = new BlockingCollection<LogMessage>();
        Task.Factory.StartNew(LogAtDatabase, TaskCreationOptions.LongRunning);
    }
 
    /// <summary>
    /// Creates a new instance of the db logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new DbLogger(this);
    }
 
    public void Dispose(){}
    
    internal void AddToQueue(LogMessage logMessage)
    {
        _queue.Add(logMessage);
    }
    
    private void LogAtDatabase()
    {
        foreach (var message in _queue.GetConsumingEnumerable())
        {
            var now = DateTime.Now;

            var options = Options.CurrentValue;

            var values = new Hashtable
            {
                [options.CreatedColumnName] = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, now.Kind),
                [options.LevelColumnName] = (int)message.LogLevel,
                [options.EventColumnName] = message.EventId.Name,
                [options.MessageColumnName] = GetMessage(message.EventId, message.Formatter(message.State, message.Exception), message.Exception),
            };
        
            var element = DbLoggerElement.GetInstance(options);
            
            if (!TableExists)
            {
                if (!Repository.TableExists(options.TableName))
                {
                    Repository.CreateDataModel(element);
                }

                TableExists = true;
            }
        
            Repository.Insert(element, values);
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