using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias(ProviderAlias)]
public sealed class DbLoggerProvider(IServiceProvider serviceProvider, IOptionsMonitor<DbLoggerOptions> options) : BatchingLoggerProvider(options)
{
    private bool _tableExists;
    private const string ProviderAlias = "Database";

    private static IEnumerable<Dictionary<string, object>> GetValues(List<LogMessage> entries, DbLoggerOptions options)
    {
        return entries.Select(entry => new Dictionary<string, object>
        {
            [options.CreatedColumnName] = entry.Timestamp.DateTime,
            [options.LevelColumnName] = entry.LogLevel,
            [options.CategoryColumnName] = entry.Category,
            [options.MessageColumnName] = entry.Message,
        });
    }

    protected override async Task WriteMessagesAsync(List<LogMessage> messages, CancellationToken token)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        var currentOptions = options.CurrentValue;
        var element = DbLoggerElement.GetInstance(currentOptions);
        var values = GetValues(messages, currentOptions);

        if (!_tableExists)
        {
            if (!await repository.TableExistsAsync(currentOptions.TableName, currentOptions.ConnectionStringId))
            {
                await repository.CreateDataModelAsync(element);
            }

            _tableExists = true;
        }
        
        await repository.BulkInsertAsync(element, values, currentOptions.ConnectionStringId);
    }
}