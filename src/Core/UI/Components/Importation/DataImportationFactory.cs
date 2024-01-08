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
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class DataImportationFactory(
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
    : IFormElementComponentFactory<JJDataImportation>
{
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private FieldsService FieldsService { get; } = fieldsService;
    private IBackgroundTaskManager BackgroundTaskManager { get; } = backgroundTaskManager;
    private FormService FormService { get; } = formService;
    private IFormEventHandlerResolver FormEventHandlerResolver { get; } = formEventHandlerResolver;
    private IHttpContext HttpContext { get; } = httpContext;
    private IComponentFactory ComponentFactory { get; } = componentFactory;

    private DataImportationWorkerFactory DataImportationWorkerFactory { get; } = dataImportationWorkerFactory;

    private IEncryptionService EncryptionService { get; } = encryptionService;
    private ILoggerFactory LoggerFactory { get; } = loggerFactory;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;


    public JJDataImportation Create(FormElement formElement)
    {
        return new JJDataImportation(formElement, ExpressionsService, FormService,
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

        dataImp.Name = elementName + "-importation";

        return dataImp;
    }
}