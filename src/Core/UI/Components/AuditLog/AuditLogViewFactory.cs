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

internal class AuditLogViewFactory : IFormElementComponentFactory<JJAuditLogView>
{
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private AuditLogService AuditLogService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IComponentFactory ComponentFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    public AuditLogViewFactory(
        IHttpContext httpContext,
        IEntityRepository entityRepository,
        AuditLogService auditLogService,
        IDataDictionaryRepository dataDictionaryRepository,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        EntityRepository = entityRepository;
        AuditLogService = auditLogService;
        DataDictionaryRepository = dataDictionaryRepository;
        ComponentFactory = componentFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }

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