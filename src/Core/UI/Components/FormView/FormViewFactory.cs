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
using JJMasterData.Core.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal sealed class FormViewFactory(
    IHttpContext currentContext,
    IEntityRepository entityRepository,
    IDataDictionaryRepository dataDictionaryRepository,
    FormService formService,
    IEncryptionService encryptionService,
    FormValuesService formValuesService,
    FieldValuesService fieldValuesService,
    ExpressionsService expressionsService,
    HtmlTemplateService htmlTemplateService,
    IEnumerable<IPluginHandler> pluginHandlers,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IOptionsSnapshot<MasterDataCoreOptions> options,
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
            formValuesService,
            fieldValuesService,
            expressionsService,
            htmlTemplateService,
            pluginHandlers,
            options,
            stringLocalizer,
            loggerFactory.CreateLogger<JJFormView>(),
            factory);
        
        return formView;
    }

    public async ValueTask<JJFormView> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        var formView = Create(formElement);
        await SetFormEventHandlerAsync(formView, formElement);
        return formView;
    }

    private ValueTask SetFormEventHandlerAsync(JJFormView formView, FormElement formElement)
    {
        var formEventHandler = formEventHandlerResolver.GetFormEventHandler(formElement.Name);
        formView.AddFormEventHandler(formEventHandler);
        if (formEventHandler != null)
        {
            return formEventHandler.OnFormElementLoadAsync(formView, new FormElementLoadEventArgs(formElement))!;
        }

        return ValueTaskHelper.CompletedTask;
    }
}