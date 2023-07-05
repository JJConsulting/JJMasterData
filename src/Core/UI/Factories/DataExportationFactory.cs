using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Components.Scripts;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Web.Factories;

public class DataExportationFactory
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IExpressionsService ExpressionsService { get; }

    private IFormFieldsService FormFieldsService { get; }
    private DataExportationScriptHelper ScriptHelper { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private IBackgroundTask BackgroundTask { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public DataExportationFactory(
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IExpressionsService expressionsService, 
        IFormFieldsService formFieldsService,
        DataExportationScriptHelper scriptHelper,
        IOptions<JJMasterDataCoreOptions> options,
        IBackgroundTask backgroundTask,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        ExpressionsService = expressionsService;
        FormFieldsService = formFieldsService;
        ScriptHelper = scriptHelper;
        Options = options;
        BackgroundTask = backgroundTask;
        StringLocalizer = stringLocalizer;
    }
    public async Task<JJDataExp> CreateDataExportationAsync(string dictionaryName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        return new JJDataExp(formElement,EntityRepository,ExpressionsService,FormFieldsService,ScriptHelper,Options, BackgroundTask, StringLocalizer);
    }

    public JJDataExp CreateDataExportation(FormElement formElement)
    {
        return new JJDataExp(formElement,EntityRepository,ExpressionsService,FormFieldsService,ScriptHelper,Options, BackgroundTask, StringLocalizer);
    }
}