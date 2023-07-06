using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

public class DataImportationFactory
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
    private UploadAreaFactory UploadAreaFactory { get; }
    private ComboBoxFactory ComboBoxFactory { get; }
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
        UploadAreaFactory uploadAreaFactory,
        ComboBoxFactory comboBoxFactory,
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

    public async Task<JJDataImp> CreateDataImportationAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));
        
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        
        var dataContext = new DataContext(HttpContext,DataContextSource.Upload, DataHelper.GetCurrentUserId(HttpContext,null));
        
        var formEvent = FormEventResolver.GetFormEvent(elementName);
        formEvent?.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(formElement));

        var dataImp = CreateDataImportation(formElement);
        
        if (formEvent != null) 
            dataImp.OnBeforeImport += formEvent.OnBeforeImport;

        return dataImp;
    }
    
    public JJDataImp CreateDataImportation(FormElement formElement)
    {
        return new JJDataImp(formElement, EntityRepository,ExpressionsService, FieldValuesService,FormService, FieldVisibilityService, BackgroundTask,HttpContext,UploadAreaFactory,ComboBoxFactory, LoggerFactory,StringLocalizer);
    }


}