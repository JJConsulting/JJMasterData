using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class DataImportationFactory : IFormElementComponentFactory<JJDataImportation>
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEntityRepository EntityRepository { get; }
    private ExpressionsService ExpressionsService { get; }
    private FieldsService FieldsService { get; }
    private IBackgroundTaskManager BackgroundTaskManager { get; }
    private FormService FormService { get; }
    private IFormEventHandlerResolver FormEventHandlerResolver { get; }
    private IHttpContext HttpContext { get; }
    private IComponentFactory ComponentFactory { get; }

    private DataImportationWorkerFactory DataImportationWorkerFactory { get; }

    private MasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private ILoggerFactory LoggerFactory { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }


    public DataImportationFactory(
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        ExpressionsService expressionsService,
        IBackgroundTaskManager backgroundTaskManager,
        FormService formService,
        FieldsService fieldsService,
        IFormEventHandlerResolver formEventHandlerResolver,
        IHttpContext httpContext,
        IComponentFactory componentFactory,
        DataImportationWorkerFactory dataImportationWorkerFactory,
        IEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FieldsService = fieldsService;
        BackgroundTaskManager = backgroundTaskManager;
        FormService = formService;
        FormEventHandlerResolver = formEventHandlerResolver;
        HttpContext = httpContext;
        ComponentFactory = componentFactory;
        DataImportationWorkerFactory = dataImportationWorkerFactory;
        EncryptionService = encryptionService;
        LoggerFactory = loggerFactory;
        StringLocalizer = stringLocalizer;
    }

    public JJDataImportation Create(FormElement formElement)
    {
        return new JJDataImportation(formElement, EntityRepository, ExpressionsService, FormService,
            FieldsService, BackgroundTaskManager, HttpContext, ComponentFactory,
            DataImportationWorkerFactory, EncryptionService, LoggerFactory,
            StringLocalizer);
    }

    public async Task<JJDataImportation> CreateAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);

        var dataContext = new DataContext(HttpContext.Request, DataContextSource.Upload,
            DataHelper.GetCurrentUserId(HttpContext, null));

        var formEvent = FormEventHandlerResolver.GetFormEventHandler(elementName);
        
        var dataImp = Create(formElement);
        
        if (formEvent != null)
        {
            await formEvent.OnFormElementLoadAsync(dataContext, new FormElementLoadEventArgs(formElement));
            
            dataImp.OnBeforeImportAsync += formEvent.OnBeforeImportAsync;
        }
        
        return dataImp;
    }
}