using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

internal sealed class DbLoggerBackgroundService(
    DbLoggerBuffer loggerBuffer,
    IOptionsMonitor<DbLoggerOptions> optionsMonitor,
    IServiceProvider serviceProvider)
    : LoggerBackgroundService<DbLoggerBuffer>(loggerBuffer)
{
    private bool TableExists { get; set; }

    protected override async Task LogAsync(LogMessage entry, CancellationToken cancellationToken)
    {
        var options = optionsMonitor.CurrentValue;
        var dbValues = GetDictionary(entry, options);
        var element = DbLoggerElement.GetInstance(options);

        using var scope = serviceProvider.CreateScope();
        var entityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        if (!TableExists)
        {
            if (!await entityRepository.TableExistsAsync(options.TableName, options.ConnectionStringId))
            {
                await entityRepository.CreateDataModelAsync(element,[]);
            }

            TableExists = true;
        }
    
        await entityRepository.InsertAsync(element, dbValues);
    }

    private static Dictionary<string, object> GetDictionary(LogMessage entry, DbLoggerOptions options)
    {
        return new Dictionary<string, object>
        {
            [options.CreatedColumnName] = entry.Created,
            [options.LevelColumnName] = entry.LogLevel,
            [options.CategoryColumnName] = entry.Category,
            [options.MessageColumnName] = entry.Message,
        };
    }

}