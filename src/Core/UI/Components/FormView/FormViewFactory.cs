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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal class FormViewFactory(
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
    ILoggerFactory loggerFactory,
    IComponentFactory factory,
    IFormEventHandlerResolver formEventHandlerResolver
    ) : IFormElementComponentFactory<JJFormView>
{
    public JJFormView Create(FormElement formElement)
    {
        var formView = new JJFormView(
            formElement,
            currentContext,
            entityRepository,
            dataDictionaryRepository,
            formService,
            encryptionService, 
            fieldValuesService, 
            expressionsService,
            pluginHandlers,
            options,
            stringLocalizer,
            loggerFactory.CreateLogger<JJFormView>(),
            factory);
        
        return formView;
    }

    public async Task<JJFormView> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        var formView = Create(formElement);
        await SetFormEventHandlerAsync(formView, formElement);
        return formView;
    }

    private Task SetFormEventHandlerAsync(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = formEventHandlerResolver.GetFormEventHandler(formElement.Name);
        formView.FormService.AddFormEventHandler(formEventHandler);
        if (formEventHandler != null)
        {
            return formEventHandler.OnFormElementLoadAsync(this, new FormElementLoadEventArgs(formElement))!;
        }

        return Task.CompletedTask;
    }
    
}