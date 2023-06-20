using System;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Entity.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

public class DbLoggerHostedService : LoggerHostedService<DbLoggerBuffer>
{
    private readonly IEntityRepository _entityRepository;
    private readonly IOptionsMonitor<DbLoggerOptions> _optionsMonitor;
    
    private bool TableExists { get; set; }
    
    public DbLoggerHostedService(DbLoggerBuffer loggerBuffer, IOptionsMonitor<DbLoggerOptions> optionsMonitor, IEntityRepository entityRepository) : base(loggerBuffer)
    {
        _entityRepository = entityRepository;
        _optionsMonitor = optionsMonitor;
    }

    protected override void Log(LogMessage message)
    {
  
        var now = DateTime.Now;

        var options = _optionsMonitor.CurrentValue;

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
            if (!_entityRepository.TableExists(options.TableName))
            {
                _entityRepository.CreateDataModel(element);
            }

            TableExists = true;
        }
    
        _entityRepository.Insert(element, values);
        
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