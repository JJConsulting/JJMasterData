using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Facades;

public class CoreServicesFacade
{
    public IFormEventResolver FormEventResolver { get; }
    public AuditLogService AuditLogService { get; }
    public IOptions<JJMasterDataCoreOptions> Options { get; }
    public ILoggerFactory LoggerFactory { get; }
    
    public IBackgroundTask BackgroundTaskManager { get; }
    
    public CoreServicesFacade(IFormEventResolver formEventResolver,
        AuditLogService auditLogService,
        IBackgroundTask backgroundTaskManager,
        IOptions<JJMasterDataCoreOptions> options,
        ILoggerFactory loggerFactory)
    {
        FormEventResolver = formEventResolver;
        AuditLogService = auditLogService;
        Options = options;
        LoggerFactory = loggerFactory;
        BackgroundTaskManager = backgroundTaskManager;
    }
}