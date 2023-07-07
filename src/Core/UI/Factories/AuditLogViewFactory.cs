using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Factories;

public class AuditLogViewFactory
{
    private Lazy<GridViewFactory> GridViewFactory { get; }
    private Lazy<DataPanelFactory> DataPanelFactory { get; }
    private IHttpContext HttpContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IAuditLogService AuditLogService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public AuditLogViewFactory(
        Lazy<GridViewFactory> gridViewFactory,
        Lazy<DataPanelFactory> dataPanelFactory,
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

    public JJAuditLogView CreateAuditLogView(FormElement formElement)
    {
        return new JJAuditLogView(formElement,GridViewFactory,DataPanelFactory, HttpContext, EntityRepository, AuditLogService,StringLocalizer);
    }
    
    public async Task<JJAuditLogView> CreateAuditLogViewAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        return CreateAuditLogView(formElement);
    }
}