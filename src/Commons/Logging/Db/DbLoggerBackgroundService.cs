using System;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

internal class DbLoggerBackgroundService : LoggerBackgroundService<DbLoggerBuffer>
{
    public readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<DbLoggerOptions> _options;
    
    private bool TableExists { get; set; }
    
    public DbLoggerBackgroundService(DbLoggerBuffer loggerBuffer, IOptionsMonitor<DbLoggerOptions> optionsMonitor,IServiceProvider serviceProvider) : base(loggerBuffer)
    {
        _serviceProvider = serviceProvider;
        _options = optionsMonitor;
    }

    protected override async Task LogAsync(char[] entry, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;

        var dbEntry = DbLogEntry.FromSeparatedString(new string(entry));

        if (dbEntry is null)
            return;
        
        var dbValues = dbEntry?.ToDictionary(options);
        
        var element = DbLoggerElement.GetInstance(options);

        using var scope = _serviceProvider.CreateScope();
        var entityRepository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
        if (!TableExists)
        {
            if (!await entityRepository.TableExistsAsync(options.TableName))
            {
                await entityRepository.CreateDataModelAsync(element);
            }

            TableExists = true;
        }
    
        await entityRepository.InsertAsync(element, dbValues);
    }
}