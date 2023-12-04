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

internal class AuditLogViewFactory(IHttpContext httpContext,
        IEntityRepository entityRepository,
        AuditLogService auditLogService,
        IDataDictionaryRepository dataDictionaryRepository,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IFormElementComponentFactory<JJAuditLogView>
{
    private IHttpContext HttpContext { get; } = httpContext;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private AuditLogService AuditLogService { get; } = auditLogService;
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private IComponentFactory ComponentFactory { get; } = componentFactory;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public JJAuditLogView Create(FormElement formElement)
    {
        return new JJAuditLogView(formElement, HttpContext, EntityRepository, AuditLogService, ComponentFactory,
            EncryptionService, StringLocalizer);
    }
    
    public async Task<JJAuditLogView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}