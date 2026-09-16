using System;
using System.Threading.Tasks;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Importation.Background;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataImportationFactory(
    IDataDictionaryRepository dataDictionaryRepository,
    ExpressionsService expressionsService,
    FieldValuesService fieldValuesService,
    FormService formService,
    IHttpContextAccessor httpContext,
    IMasterDataUser masterDataUser,
    IComponentFactory componentFactory,
    DataItemService dataItemService,
    ImportJobService importJobService,
    IFileStorage fileStorage,
    DataProtectionService encryptionService,
    ILoggerFactory loggerFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : IFormElementComponentFactory<JJDataImportation>
{
    public JJDataImportation Create(FormElement formElement)
    {
        return new JJDataImportation(
            formElement, 
            masterDataUser,
            expressionsService, 
            formService, 
            fieldValuesService,
            httpContext, 
            componentFactory, 
            dataItemService,
            importJobService,
            fileStorage,
            encryptionService,
            loggerFactory,
            stringLocalizer);
    }

    public async ValueTask<JJDataImportation> CreateAsync(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);

        var dataImp = Create(formElement);

        dataImp.Name = elementName + "-importation";

        return dataImp;
    }
}
