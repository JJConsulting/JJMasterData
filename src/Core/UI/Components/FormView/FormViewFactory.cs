using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal class FormViewFactory : IFormElementComponentFactory<JJFormView>
{
    private IHttpContext CurrentContext { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private FormService FormService { get; }
    private IEncryptionService EncryptionService { get; }
    private FieldValuesService FieldValuesService { get; }
    private ExpressionsService ExpressionsService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private IOptions<MasterDataCoreOptions> Options { get; }
    private IEnumerable<IPluginHandler> PluginHandlers { get; }
    private IComponentFactory Factory { get; }
    private IFormEventHandlerResolver FormEventHandlerResolver { get; }

    
    public FormViewFactory(
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        FormService formService,
        IEncryptionService encryptionService,
        FieldValuesService fieldValuesService,
        ExpressionsService expressionsService,
        IEnumerable<IPluginHandler> pluginHandlers,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IOptions<MasterDataCoreOptions> options,
        IComponentFactory factory,
        IFormEventHandlerResolver formEventHandlerResolver
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
        FormEventHandlerResolver = formEventHandlerResolver;
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
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var formView = Create(formElement);
        await SetFormEventHandlerAsync(formView, formElement);
        return formView;
    }

    private void SetFormEventHandler(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = FormEventHandlerResolver.GetFormEventHandler(formElement.Name);
        formView.FormService.AddFormEventHandler(formEventHandler);

        formEventHandler?.OnFormElementLoad(this, new FormElementLoadEventArgs(formElement));
    }
    
    internal async Task SetFormEventHandlerAsync(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = FormEventHandlerResolver.GetFormEventHandler(formElement.Name);
        formView.FormService.AddFormEventHandler(formEventHandler);

        if (formEventHandler != null)
        {
            await formEventHandler.OnFormElementLoadAsync(this, new FormElementLoadEventArgs(formElement))!;
        }
    }
    
}