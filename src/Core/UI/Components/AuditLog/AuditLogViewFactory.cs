using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Factories;

internal class AuditLogViewFactory : IFormElementComponentFactory<JJAuditLogView>
{
    private Lazy<IFormElementComponentFactory<JJGridView>> GridViewFactory { get; }
    private Lazy<IFormElementComponentFactory<JJDataPanel>> DataPanelFactory { get; }
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IAuditLogService AuditLogService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public AuditLogViewFactory(
        Lazy<IFormElementComponentFactory<JJGridView>> gridViewFactory,
        Lazy<IFormElementComponentFactory<JJDataPanel>> dataPanelFactory,
        IHttpContext httpContext,
        IEntityRepository entityRepository,
        IAuditLogService auditLogService,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        GridViewFactory = gridViewFactory;
        DataPanelFactory = dataPanelFactory;
        HttpContext = httpContext;
        EntityRepository = entityRepository;
        AuditLogService = auditLogService;
        DataDictionaryRepository = dataDictionaryRepository;
        StringLocalizer = stringLocalizer;
    }

    public JJAuditLogView Create(FormElement formElement)
    {
        return new JJAuditLogView(formElement,GridViewFactory,DataPanelFactory, HttpContext, EntityRepository, AuditLogService,StringLocalizer);
    }
    
    public async Task<JJAuditLogView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        return Create(formElement);
    }
}