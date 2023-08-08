using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Services;

namespace JJMasterData.Core.Web.Factories;

internal class FormViewFactory : IFormElementComponentFactory<JJFormView>
{
    private IHttpContext CurrentContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryService DataDictionaryService { get; }
    private IFormService FormService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IFieldValuesService FieldValuesService { get; }
    private IExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ComponentFactory Factory { get; }
    private IFormEventHandlerFactory FormEventHandlerFactory { get; }

    public FormViewFactory(
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryService dataDictionaryService,
        IFormService formService,
        JJMasterDataEncryptionService encryptionService,
        IFieldValuesService fieldValuesService,
        IExpressionsService expressionsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ComponentFactory factory,
        IFormEventHandlerFactory formEventHandlerFactory
    )
    {
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        DataDictionaryService = dataDictionaryService;
        FormService = formService;
        EncryptionService = encryptionService;
        FieldValuesService = fieldValuesService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        Factory = factory;
        FormEventHandlerFactory = formEventHandlerFactory;
    }

    public JJFormView Create(FormElement formElement)
    {
        var formView = new JJFormView(
            formElement,
            CurrentContext,
            EntityRepository, DataDictionaryService, FormService,
            EncryptionService, FieldValuesService, ExpressionsService,
            StringLocalizer,
            Factory)
        {
            IsExternalRoute = true
        };

        return formView;
    }

    public async Task<JJFormView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryService.GetMetadataAsync(elementName);
        var form = Create(formElement);
        await SetFormViewParamsAsync(form, formElement);
        return form;
    }

    internal async Task SetFormViewParamsAsync(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = FormEventHandlerFactory.GetFormEvent(formElement.Name);
        formView.FormService.AddFormEventHandler(formEventHandler);

        formView.Name = "jj-form-view-" + formElement.Name.ToLower();

        if (formEventHandler != null)
        {
            // ReSharper disable once MethodHasAsyncOverload
            formEventHandler.OnFormElementLoad(this, new FormElementLoadEventArgs(formElement));
                    
            await formEventHandler.OnFormElementLoadAsync(this, new FormElementLoadEventArgs(formElement))!;
        }

        
        SetFormOptions(formView, formElement.Options);
    }

    internal static void SetFormOptions(JJFormView formView, FormElementOptions metadataOptions)
    {
        if (metadataOptions == null)
            return;
        
        formView.ShowTitle = metadataOptions.Grid.ShowTitle;
        formView.DataPanel.FormUI = metadataOptions.Form;
    }
    
}