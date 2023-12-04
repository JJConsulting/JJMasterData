using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationFactory(
    IDataDictionaryRepository dataDictionaryRepository,
    ExpressionsService expressionsService,
    FieldsService fieldsService,
    IOptions<MasterDataCoreOptions> options,
    IBackgroundTaskManager backgroundTaskManager,
    IHttpContext httpContext,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILoggerFactory loggerFactory,
    IComponentFactory componentFactory,
    MasterDataUrlHelper urlHelper,
    IEncryptionService encryptionService,
    DataExportationWriterFactory dataExportationWriterFactory
        ) : IFormElementComponentFactory<JJDataExportation>
{
    public async Task<JJDataExportation> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }

    public JJDataExportation Create(FormElement formElement)
    {
        return new JJDataExportation(
            formElement,
            expressionsService,
            fieldsService, 
            options, 
            backgroundTaskManager,
            stringLocalizer, 
            componentFactory,
            loggerFactory, 
            httpContext, 
            urlHelper,
            encryptionService,
            dataExportationWriterFactory);
    }
}