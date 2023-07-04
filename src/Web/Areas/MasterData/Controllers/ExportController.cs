using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Http.Abstractions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class ExportController : MasterDataController
{
    private GridViewFactory GridViewFactory { get; }
    private DataExportationFactory DataExportationFactory { get; }

    public ExportController(GridViewFactory gridViewFactory,DataExportationFactory dataExportationFactory)
    {
        GridViewFactory = gridViewFactory;
        DataExportationFactory = dataExportationFactory;
    }
    
    [DictionaryNameDecryptionServiceFilter]
    public async Task<IActionResult> StartExportation([FromQuery]string dictionaryName)
    {
        var gridView = await GridViewFactory.CreateGridViewAsync(dictionaryName);
        
        gridView.ExportFileInBackground();
        
        var html = new DataExpLog(gridView.DataExportation.Name).GetHtmlProcess().ToString();
        
        return Content(html);
    }
    
    [DictionaryNameDecryptionServiceFilter]
    public async Task<IActionResult> Settings([FromQuery]string dictionaryName)
    {
        var dataExportation = await DataExportationFactory.CreateDataExportationAsync(dictionaryName);
        var settings = new DataExpSettings(dataExportation);
        return Content(settings.GetHtmlElement().ToString());
    }
    
    [DictionaryNameDecryptionServiceFilter]
    public async Task<IActionResult> CheckProgress(string dictionaryName)
    {
        var dataExportation = await DataExportationFactory.CreateDataExportationAsync(dictionaryName);
        var progress = dataExportation.GetCurrentProgress();
        
        return Json(progress);
    }

}