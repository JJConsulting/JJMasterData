using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

//I think Exportation is better than Exp, exp can be experience, experiment, etc.
public class DataExportationFactory
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IExpressionsService ExpressionsService { get; }

    private IFormFieldsService FormFieldsService { get; }
    private IBackgroundTask BackgroundTask { get; }
    
    public DataExportationFactory(
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IExpressionsService expressionsService, 
        IFormFieldsService formFieldsService,
        IBackgroundTask backgroundTask)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        ExpressionsService = expressionsService;
        FormFieldsService = formFieldsService;
        BackgroundTask = backgroundTask;
    }
    public async Task<JJDataExp> CreateDataExportationAsync(string dictionaryName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        return new JJDataExp(formElement,EntityRepository,ExpressionsService,FormFieldsService,BackgroundTask);
    }

    public JJDataExp CreateDataExportation(FormElement formElement)
    {
        return new JJDataExp(formElement,EntityRepository,ExpressionsService,FormFieldsService,BackgroundTask);
    }
}