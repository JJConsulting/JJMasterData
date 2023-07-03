using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Entity.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

public class DbLoggerBackgroundService : LoggerBackgroundService<DbLoggerBuffer>
{
    private readonly IEntityRepository _entityRepository;
    private readonly IOptionsMonitor<DbLoggerOptions> _optionsMonitor;
    
    private bool TableExists { get; set; }
    
    public DbLoggerBackgroundService(DbLoggerBuffer loggerBuffer, IOptionsMonitor<DbLoggerOptions> optionsMonitor, IEntityRepository entityRepository) : base(loggerBuffer)
    {
        _entityRepository = entityRepository;
        _optionsMonitor = optionsMonitor;
    }

    protected override async Task LogAsync(LogMessage message, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        var options = _optionsMonitor.CurrentValue;

        var values = new Dictionary<string,dynamic>
        {
            [options.CreatedColumnName] = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, now.Kind),
            [options.LevelColumnName] = (int)message.LogLevel,
            [options.EventColumnName] = message.EventId.Name,
            [options.MessageColumnName] = GetMessage(message.EventId, message.Formatter(message.State, message.Exception), message.Exception),
        };
    
        var element = DbLoggerElement.GetInstance(options);
        
        if (!TableExists)
        {
            if (!await _entityRepository.TableExistsAsync(options.TableName))
            {
                await _entityRepository.CreateDataModelAsync(element);
            }

            TableExists = true;
        }
    
        await _entityRepository.InsertAsync(element, values);
        
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