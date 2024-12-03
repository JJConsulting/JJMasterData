using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias(ProviderAlias)]
public class DbLoggerProvider(IServiceProvider serviceProvider, IOptionsMonitor<DbLoggerOptions> options) : BatchingLoggerProvider(options)
{
    private bool _tableExists;
    private const string ProviderAlias = "Database";
    private async Task LogAsync(LogMessage entry, CancellationToken cancellationToken)
    {
        var currentOptions = options.CurrentValue;
        var dbValues = GetDictionary(entry, currentOptions);
        var element = DbLoggerElement.GetInstance(currentOptions);

        using var scope = serviceProvider.CreateScope();
        var entityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        if (!_tableExists)
        {
            if (!await entityRepository.TableExistsAsync(currentOptions.TableName, currentOptions.ConnectionStringId))
            {
                await entityRepository.CreateDataModelAsync(element,[]);
            }

            _tableExists = true;
        }
    
        await entityRepository.InsertAsync(element, dbValues);
    }

    private static Dictionary<string, object> GetDictionary(LogMessage entry, DbLoggerOptions options)
    {
        return new Dictionary<string, object>
        {
            [options.CreatedColumnName] = entry.Timestamp.DateTime,
            [options.LevelColumnName] = entry.LogLevel,
            [options.CategoryColumnName] = entry.Category,
            [options.MessageColumnName] = entry.Message,
        };
    }
    protected override async Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken token)
    {
        foreach (var message in messages)
        {
            await LogAsync(message, token);
        }
    }
}