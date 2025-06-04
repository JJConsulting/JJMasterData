using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class AuditLogViewFactory(IHttpContext httpContext,
        IEntityRepository entityRepository,
        AuditLogService auditLogService,
        IDataDictionaryRepository dataDictionaryRepository,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IFormElementComponentFactory<JJAuditLogView>
{
    public JJAuditLogView Create(FormElement formElement)
    {
        return new JJAuditLogView(formElement, httpContext, entityRepository, auditLogService, componentFactory,
            encryptionService, stringLocalizer);
    }
    
    public async ValueTask<JJAuditLogView> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}