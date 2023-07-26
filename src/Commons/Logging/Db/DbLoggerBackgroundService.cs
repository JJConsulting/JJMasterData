using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

internal class DbLoggerBackgroundService : LoggerBackgroundService<DbLoggerBuffer>
{
    private readonly IEntityRepository _entityRepository;
    private readonly IOptionsMonitor<DbLoggerOptions> _options;
    
    private bool TableExists { get; set; }
    
    public DbLoggerBackgroundService(DbLoggerBuffer loggerBuffer, IOptionsMonitor<DbLoggerOptions> optionsMonitor, IEntityRepository entityRepository) : base(loggerBuffer)
    {
        _entityRepository = entityRepository;
        _options = optionsMonitor;
    }

    protected override async Task LogAsync(char[] entry, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;

        var dbEntry = DbLogEntry.FromSeparatedString(new string(entry));

        var dbValues = dbEntry.ToDictionary(options);
        
        var element = DbLoggerElement.GetInstance(options);
        
        if (!TableExists)
        {
            if (!await _entityRepository.TableExistsAsync(options.TableName))
            {
                await _entityRepository.CreateDataModelAsync(element);
            }

            TableExists = true;
        }
    
        await _entityRepository.InsertAsync(element, dbValues);
    }
}