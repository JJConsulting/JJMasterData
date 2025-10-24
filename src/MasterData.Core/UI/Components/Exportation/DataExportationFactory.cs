using System.Threading.Tasks;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationFactory(
    IDataDictionaryRepository dataDictionaryRepository,
    ExpressionsService expressionsService,
    IOptionsSnapshot<MasterDataCoreOptions> options,
    IBackgroundTaskManager backgroundTaskManager,
    IHttpContext httpContext,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILoggerFactory loggerFactory,
    IComponentFactory componentFactory,
    IEncryptionService encryptionService,
    DataExportationWriterFactory dataExportationWriterFactory
        ) : IFormElementComponentFactory<JJDataExportation>
{
    public async ValueTask<JJDataExportation> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }

    public JJDataExportation Create(FormElement formElement)
    {
        return new JJDataExportation(
            formElement,
            expressionsService,
            options, 
            backgroundTaskManager,
            stringLocalizer, 
            componentFactory,
            loggerFactory, 
            httpContext,
            encryptionService,
            dataExportationWriterFactory);
    }
}