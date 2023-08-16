using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class AuditLogFormElementFactory : IFormElementFactory
{
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private IAuditLogService AuditLogService { get; }

    public AuditLogFormElementFactory(IOptions<JJMasterDataCoreOptions> options, IAuditLogService auditLogService)
    {
        Options = options;
        AuditLogService = auditLogService;
    }

    public string ElementName => Options.Value.AuditLogTableName;
    public FormElement GetFormElement()
    {
        return AuditLogService.GetFormElement();
    }
}