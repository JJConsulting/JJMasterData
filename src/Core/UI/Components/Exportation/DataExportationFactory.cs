using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationFactory : IFormElementComponentFactory<JJDataExportation>
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private ExpressionsService ExpressionsService { get; }
    private FieldsService FieldsService { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private IBackgroundTaskManager BackgroundTaskManager { get; }
    private IHttpContext HttpContext { get; }
    
    private IComponentFactory ComponentFactory { get; }
    private DataExportationWriterFactory DataExportationWriterFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public DataExportationFactory(
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        IOptions<JJMasterDataCoreOptions> options,
        IBackgroundTaskManager backgroundTaskManager,
        IHttpContext httpContext,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory,
        IComponentFactory componentFactory,
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        DataExportationWriterFactory dataExportationWriterFactory
        )
    {
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        ExpressionsService = expressionsService;
        FieldsService = fieldsService;
        Options = options;
        BackgroundTaskManager = backgroundTaskManager;
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
        ComponentFactory = componentFactory;
        DataExportationWriterFactory = dataExportationWriterFactory;
    }

    public async Task<JJDataExportation> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }

    public JJDataExportation Create(FormElement formElement)
    {
        return new JJDataExportation(
            formElement,
            EntityRepository, 
            ExpressionsService,
            FieldsService, 
            Options, 
            BackgroundTaskManager,
            StringLocalizer, 
            ComponentFactory,
            LoggerFactory, 
            HttpContext, 
            UrlHelper,
            EncryptionService,
            DataExportationWriterFactory);
    }
}