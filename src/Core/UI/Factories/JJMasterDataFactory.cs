using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public class JJMasterDataFactory
{
    internal FormViewFactory FormViewFactory { get; }
    internal DataPanelFactory DataPanelFactory { get; }
    internal GridViewFactory GridViewFactory { get; }
    internal DataExportationFactory DataExportationFactory { get; }
    internal DataImportationFactory DataImportationFactory { get; }
    internal ComboBoxFactory ComboBoxFactory { get; }
    internal SearchBoxFactory SearchBoxFactory { get; }
    internal LookupFactory LookupFactory { get; }

    public JJMasterDataFactory(
        FormViewFactory formViewFactory,
        DataPanelFactory dataPanelFactory,
        GridViewFactory gridViewFactory,
        DataExportationFactory dataExportationFactory,
        DataImportationFactory dataImportationFactory,
        ComboBoxFactory comboBoxFactory,
        SearchBoxFactory searchBoxFactory,
        LookupFactory lookupFactory
        )
    {
        FormViewFactory = formViewFactory;
        DataPanelFactory = dataPanelFactory;
        GridViewFactory = gridViewFactory;
        DataExportationFactory = dataExportationFactory;
        DataImportationFactory = dataImportationFactory;
        ComboBoxFactory = comboBoxFactory;
        SearchBoxFactory = searchBoxFactory;
        LookupFactory = lookupFactory;
    }
    
    #if NET48
    public static JJMasterDataFactory GetInstance()
    {
        return new JJMasterDataFactory(
            JJService.Provider.GetScopedDependentService<FormViewFactory>(),
            JJService.Provider.GetScopedDependentService<DataPanelFactory>(),
            JJService.Provider.GetScopedDependentService<GridViewFactory>(),
            JJService.Provider.GetScopedDependentService<DataExportationFactory>(),
            JJService.Provider.GetScopedDependentService<DataImportationFactory>(),
            JJService.Provider.GetScopedDependentService<ComboBoxFactory>(),
            JJService.Provider.GetScopedDependentService<SearchBoxFactory>(), 
            JJService.Provider.GetScopedDependentService<LookupFactory>());
    }
    #endif
    
    public JJComboBox CreateComboBox()
    {
        return ComboBoxFactory.CreateComboBox();
    }
    
    public JJSearchBox CreateSearchBox()
    {
        return  SearchBoxFactory.CreateSearchBox();
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
        return DataImportationFactory.CreateDataImportation(formElement);
    }
    
    public JJFormView CreateFormView(FormElement formElement)
    {
        return FormViewFactory.CreateFormView(formElement);
    }
    
    public async Task<JJFormView> CreateFormViewAsync(string dictionaryName)
    {
        return await FormViewFactory.CreateFormViewAsync(dictionaryName);
    }

    public JJDataPanel CreateDataPanel(FormElement formElement)
    {
        return DataPanelFactory.CreateDataPanel(formElement);
    }
    
    public async Task<JJDataPanel> CreateDataPanelAsync(string dictionaryName)
    {
        return await DataPanelFactory.CreateDataPanelAsync(dictionaryName);
    }
    
    public JJGridView CreateGridView(FormElement formElement)
    {
        return GridViewFactory.CreateGridView(formElement);
    }
    
    public async Task<JJGridView> CreateGridViewAsync(string dictionaryName)
    {
        return await GridViewFactory.CreateGridViewAsync(dictionaryName);
    }
}