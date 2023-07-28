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

namespace JJMasterData.Core.Web.Factories;

internal class DataImportationFactory : IFormElementComponentFactory<JJDataImp>
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    private IFieldValuesService FieldValuesService { get; }
    private IBackgroundTask BackgroundTask { get; }
    private IFormService FormService { get; }
    private IFieldVisibilityService FieldVisibilityService { get; }
    private IFormEventResolver FormEventResolver { get; }
    private IHttpContext HttpContext { get; }
    private  IComponentFactory<JJUploadArea> UploadAreaFactory { get; }
    private  IControlFactory<JJComboBox> ComboBoxFactory { get; }
    private ILoggerFactory LoggerFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public DataImportationFactory(
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        IFieldValuesService fieldValuesService,
        IBackgroundTask backgroundTask,
        IFormService formService,
        IFieldVisibilityService fieldVisibilityService,
        IFormEventResolver formEventResolver,
        IHttpContext httpContext,
        IComponentFactory<JJUploadArea> uploadAreaFactory,
        IControlFactory<JJComboBox> comboBoxFactory,
        ILoggerFactory loggerFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        FieldValuesService = fieldValuesService;
        BackgroundTask = backgroundTask;
        FormService = formService;
        FieldVisibilityService = fieldVisibilityService;
        FormEventResolver = formEventResolver;
        HttpContext = httpContext;
        UploadAreaFactory = uploadAreaFactory;
        ComboBoxFactory = comboBoxFactory;
        LoggerFactory = loggerFactory;
        StringLocalizer = stringLocalizer;
    }

    public JJDataImp Create(FormElement formElement)
    {
        return new JJDataImp(formElement, EntityRepository, ExpressionsService, FieldValuesService, FormService,
            FieldVisibilityService, BackgroundTask, HttpContext, UploadAreaFactory, ComboBoxFactory, LoggerFactory,
            StringLocalizer);
    }

    public async Task<JJDataImp> CreateAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

        var dataContext = new DataContext(HttpContext, DataContextSource.Upload,
            DataHelper.GetCurrentUserId(HttpContext, null));

        var formEvent = FormEventResolver.GetFormEvent(elementName);
        formEvent?.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(formElement));

        var dataImp = Create(formElement);

        if (formEvent != null)
            dataImp.OnBeforeImport += formEvent.OnBeforeImport;

        return dataImp;
    }
}