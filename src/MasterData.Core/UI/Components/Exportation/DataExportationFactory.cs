using System.Threading.Tasks;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Security;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Background;
using JJMasterData.Core.DataManager.Expressions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationFactory(
    IDataDictionaryRepository dataDictionaryRepository,
    IMasterDataUser masterDataUser,
    IUrlHelper urlHelper,
    ExpressionsService expressionsService,
    IOptionsSnapshot<MasterDataCoreOptions> options,
    IHttpContextAccessor httpContext,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILoggerFactory loggerFactory,
    IComponentFactory componentFactory,
    DataProtectionService encryptionService,
    IFileStorage fileStorage,
    ExportJobService exportJobService,
    ExportFormatCatalog exportFormatCatalog
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
            masterDataUser,
            urlHelper,
            expressionsService,
            options, 
            stringLocalizer, 
            componentFactory,
            loggerFactory, 
            httpContext,
            encryptionService,
            fileStorage,
            exportJobService,
            exportFormatCatalog);
    }
}
