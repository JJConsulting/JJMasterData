using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.GridView;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class ExportationController : MasterDataController
{
    private IFormElementComponentFactory<JJGridView> GridViewFactory { get; }
    private IFormElementComponentFactory<JJDataExportation>  DataExportationFactory { get; }

    public ExportationController(IFormElementComponentFactory<JJGridView> gridViewFactory,IFormElementComponentFactory<JJDataExportation> dataExportationFactory)
    {
        GridViewFactory = gridViewFactory;
        DataExportationFactory = dataExportationFactory;
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> StartExportation(FormElement formElement, string componentName)
    {
        var gridView = GridViewFactory.Create(formElement);
        gridView.IsExternalRoute = true;
        gridView.DataExportation.Name = componentName;
        await gridView.ExportFileInBackground();
        
        var html = new DataExportationLog(gridView.DataExportation).GetHtmlProcess().ToString();
        
        return Content(html);
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult StopExportation(FormElement formElement, string componentName)
    {
        var dataExportation = DataExportationFactory.Create(formElement);
        dataExportation.Name = componentName;
        dataExportation.IsExternalRoute = true;
        dataExportation.StopExportation();

        return Json(new {});
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult Settings(FormElement formElement, string componentName)
    {
        var dataExportation =  DataExportationFactory.Create(formElement);
        dataExportation.Name = componentName;
        dataExportation.IsExternalRoute = true;
        
        var settings = new DataExportationSettings(dataExportation);
        return Content(settings.GetHtmlBuilder().ToString());
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    public IActionResult CheckProgress(FormElement formElement, string componentName)
    {
        var dataExportation =  DataExportationFactory.Create(formElement);
        dataExportation.Name = componentName;
        dataExportation.IsExternalRoute = true;
        
        var progress = dataExportation.GetCurrentProgress();
        
        return Json(progress);
    }

}