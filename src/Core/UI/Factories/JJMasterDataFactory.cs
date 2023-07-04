using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public class JJMasterDataFactory
{
    private DataExportationFactory DataExportationFactory { get; }
    private DataImportationFactory DataImportationFactory { get; }
    private SearchBoxFactory SearchBoxFactory { get; }
    public JJMasterDataFactory(
        DataExportationFactory dataExportationFactory,
        DataImportationFactory dataImportationFactory,
        SearchBoxFactory searchBoxFactory)
    {
        DataExportationFactory = dataExportationFactory;
        DataImportationFactory = dataImportationFactory;
        SearchBoxFactory = searchBoxFactory;
    }
    
    public static JJMasterDataFactory GetInstance()
    {
        return new JJMasterDataFactory(
            JJService.Provider.GetScopedDependentService<DataExportationFactory>(),
            JJService.Provider.GetScopedDependentService<DataImportationFactory>(),
            JJService.Provider.GetScopedDependentService<SearchBoxFactory>() );
    }

    public async Task<JJSearchBox> CreateSearchBoxAsync(string dictionaryName, string fieldName, PageState pageState, IDictionary<string,dynamic> userValues)
    {
        return await SearchBoxFactory.CreateSearchBoxAsync(dictionaryName, fieldName, pageState, userValues);
    }
    
    public JJDataExp CreateDataExportation(FormElement formElement)
    {
        return DataExportationFactory.CreateDataExportation(formElement);
    }
    
    public async Task<JJDataExp> CreateDataExportationAsync(string dictionaryName)
    {
        return await DataExportationFactory.CreateDataExportationAsync(dictionaryName);
    }
    
    public JJDataImp CreateDataImportation(FormElement formElement)
    {
        return DataImportationFactory.CreateDataImp(formElement);
    }

}