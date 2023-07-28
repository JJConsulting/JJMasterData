using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Factories;

internal class AuditLogViewFactory : IFormElementComponentFactory<JJAuditLogView>
{
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IAuditLogService AuditLogService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private ComponentFactory ComponentFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public AuditLogViewFactory(
        IHttpContext httpContext,
        IEntityRepository entityRepository,
        IAuditLogService auditLogService,
        IDataDictionaryRepository dataDictionaryRepository,
        ComponentFactory componentFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        EntityRepository = entityRepository;
        AuditLogService = auditLogService;
        DataDictionaryRepository = dataDictionaryRepository;
        ComponentFactory = componentFactory;
        StringLocalizer = stringLocalizer;
    }

    public JJAuditLogView Create(FormElement formElement)
    {
        return new JJAuditLogView(formElement, HttpContext, EntityRepository, AuditLogService, ComponentFactory,StringLocalizer);
    }
    
    public async Task<JJAuditLogView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        return Create(formElement);
    }
}