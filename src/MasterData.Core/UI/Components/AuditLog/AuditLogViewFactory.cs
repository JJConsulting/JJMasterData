using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class AuditLogViewFactory(
        IHttpContextAccessor httpContext,
        IMasterDataUser masterDataUser,
        IEntityRepository entityRepository,
        AuditLogService auditLogService,
        DataDictionaryService dataDictionaryService,
        IComponentFactory componentFactory,
        DataProtectionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IFormElementComponentFactory<JJAuditLogView>
{
    public JJAuditLogView Create(FormElement formElement)
    {
        return new JJAuditLogView(formElement, httpContext,masterDataUser, entityRepository, auditLogService, componentFactory,
            encryptionService, stringLocalizer);
    }
    
    public async ValueTask<JJAuditLogView> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryService.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}
