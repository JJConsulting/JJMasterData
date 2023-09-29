using System.Collections.Generic;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Options;
using ActionsService = JJMasterData.Core.DataDictionary.Services.ActionsService;

namespace JJMasterData.Core.Web.Factories;

internal class FormViewFactory : IFormElementComponentFactory<JJFormView>
{
    private IHttpContext CurrentContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private FormService FormService { get; }
    private IEncryptionService EncryptionService { get; }
    private FieldValuesService FieldValuesService { get; }
    private ExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private IEnumerable<IPluginHandler> PluginHandlers { get; }
    private IComponentFactory Factory { get; }
    private IFormEventHandlerFactory FormEventHandlerFactory { get; }

    
    public FormViewFactory(
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        FormService formService,
        IEncryptionService encryptionService,
        FieldValuesService fieldValuesService,
        ExpressionsService expressionsService,
        IEnumerable<IPluginHandler> pluginHandlers,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IOptions<JJMasterDataCoreOptions> options,
        IComponentFactory factory,
        IFormEventHandlerFactory formEventHandlerFactory
    )
    {
        CurrentContext = currentContext;
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        FormService = formService;
        EncryptionService = encryptionService;
        FieldValuesService = fieldValuesService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        Options = options;
        Factory = factory;
        FormEventHandlerFactory = formEventHandlerFactory;
        PluginHandlers = pluginHandlers;
    }

    public JJFormView Create(FormElement formElement)
    {
        var formView = new JJFormView(
            formElement,
            CurrentContext,
            EntityRepository,
            DataDictionaryRepository,
            FormService,
            EncryptionService, 
            FieldValuesService, 
            ExpressionsService,
            PluginHandlers,
            Options,
            StringLocalizer,
            Factory);
        
        SetFormEventHandler(formView, formElement);
        
        return formView;
    }



    public async Task<JJFormView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        var formView = Create(formElement);
        await SetFormEventHandlerAsync(formView, formElement);
        return formView;
    }

    private void SetFormEventHandler(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = FormEventHandlerFactory.GetFormEvent(formElement.Name);
        formView.FormService.AddFormEventHandler(formEventHandler);

        formEventHandler?.OnFormElementLoad(this, new FormElementLoadEventArgs(formElement));
    }
    
    internal async Task SetFormEventHandlerAsync(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = FormEventHandlerFactory.GetFormEvent(formElement.Name);
        formView.FormService.AddFormEventHandler(formEventHandler);

        if (formEventHandler != null)
        {
            await formEventHandler.OnFormElementLoadAsync(this, new FormElementLoadEventArgs(formElement))!;
        }
    }
    
}