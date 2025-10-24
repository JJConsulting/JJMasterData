using System;
using System.Threading.Tasks;
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

internal sealed class DataImportationFactory(
    IDataDictionaryRepository dataDictionaryRepository,
    IFormEventHandlerResolver formEventHandlerResolver,
    ExpressionsService expressionsService,
    FieldValuesService fieldValuesService,
    FormService formService,
    IBackgroundTaskManager backgroundTaskManager,
    IHttpContext httpContext,
    IComponentFactory componentFactory,
    DataItemService dataItemService,
    DataImportationWorkerFactory dataImportationWorkerFactory,
    IEncryptionService encryptionService,
    ILoggerFactory loggerFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : IFormElementComponentFactory<JJDataImportation>
{
    public JJDataImportation Create(FormElement formElement)
    {
        return new JJDataImportation(
            formElement, 
            expressionsService, 
            formService, 
            fieldValuesService,
            backgroundTaskManager,
            httpContext, 
            componentFactory, 
            dataItemService,
            dataImportationWorkerFactory,
            encryptionService,
            loggerFactory,
            stringLocalizer);
    }

    public async ValueTask<JJDataImportation> CreateAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);

        var dataContext = new DataContext(httpContext.Request, DataContextSource.Upload,
            DataHelper.GetCurrentUserId(httpContext, null));

        var formEvent = formEventHandlerResolver.GetFormEventHandler(elementName);

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