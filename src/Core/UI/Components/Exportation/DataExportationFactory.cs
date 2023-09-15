using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exports;
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class DataExportationFactory : IFormElementComponentFactory<JJDataExportation>
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    private IFieldsService FieldsService { get; }
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
        IExpressionsService expressionsService,
        IFieldsService fieldsService,
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

    public async Task<JJDataExportation> CreateAsync(string dictionaryName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
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