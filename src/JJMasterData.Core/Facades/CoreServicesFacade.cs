using System.Collections.Generic;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Facades;

public class CoreServicesFacade
{
    public IEnumerable<IWriter> ExportationWriters { get; }
    public IFormEventResolver FormEventResolver { get; }
    public AuditLogService AuditLogService { get; }
    public IOptions<JJMasterDataCoreOptions> Options { get; }
    public ILoggerFactory LoggerFactory { get; }

    public CoreServicesFacade(
        IEnumerable<IWriter> exportationWriters,
        IFormEventResolver formEventResolver,
        AuditLogService auditLogService, 
        IOptions<JJMasterDataCoreOptions> options, 
        ILoggerFactory loggerFactory)
    {
        ExportationWriters = exportationWriters;
        FormEventResolver = formEventResolver;
        AuditLogService = auditLogService;
        Options = options;
        LoggerFactory = loggerFactory;
    }
}