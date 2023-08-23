using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Components.Importation;

namespace JJMasterData.Core.Web.Factories;

internal class DataImportationFactory : IFormElementComponentFactory<JJDataImportation>
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    private IFieldsService FieldsService { get; }
    private IBackgroundTask BackgroundTask { get; }
    private IFormService FormService { get; }
    private IFormEventHandlerFactory FormEventHandlerFactory { get; }
    private IHttpContext HttpContext { get; }
    private IComponentFactory<JJUploadArea> UploadAreaFactory { get; }
    private IControlFactory<JJComboBox> ComboBoxFactory { get; }

    private DataImportationWorkerFactory DataImportationWorkerFactory { get; }

    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private ILoggerFactory LoggerFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }


    public DataImportationFactory(
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        IBackgroundTask backgroundTask,
        IFormService formService,
        IFieldsService fieldsService,
        IFormEventHandlerFactory formEventHandlerFactory,
        IHttpContext httpContext,
        IComponentFactory<JJUploadArea> uploadAreaFactory,
        IControlFactory<JJComboBox> comboBoxFactory,
        DataImportationWorkerFactory dataImportationWorkerFactory,
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FieldsService = fieldsService;
        BackgroundTask = backgroundTask;
        FormService = formService;
        FormEventHandlerFactory = formEventHandlerFactory;
        HttpContext = httpContext;
        UploadAreaFactory = uploadAreaFactory;
        ComboBoxFactory = comboBoxFactory;
        DataImportationWorkerFactory = dataImportationWorkerFactory;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        LoggerFactory = loggerFactory;
        StringLocalizer = stringLocalizer;
    }

    public JJDataImportation Create(FormElement formElement)
    {
        return new JJDataImportation(formElement, EntityRepository, ExpressionsService, FormService,
            FieldsService, BackgroundTask, HttpContext, UploadAreaFactory, ComboBoxFactory,
            DataImportationWorkerFactory, UrlHelper, EncryptionService, LoggerFactory,
            StringLocalizer);
    }

    public async Task<JJDataImportation> CreateAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

        var dataContext = new DataContext(HttpContext, DataContextSource.Upload,
            DataHelper.GetCurrentUserId(HttpContext, null));

        var formEvent = FormEventHandlerFactory.GetFormEvent(elementName);
        
        var dataImp = Create(formElement);
        
        if (formEvent != null)
        {
            // ReSharper disable once MethodHasAsyncOverload
            formEvent.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(formElement));
            await formEvent.OnFormElementLoadAsync(dataContext, new FormElementLoadEventArgs(formElement));
            
            dataImp.OnBeforeImport += formEvent.OnBeforeImport;
        }
        
        return dataImp;
    }
}