using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Http.Abstractions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class ExportController : MasterDataController
{
    private DataExportationFactory DataExportationFactory { get; }

    public ExportController(DataExportationFactory dataExportationFactory)
    {
        DataExportationFactory = dataExportationFactory;
    }
    
    public IActionResult Settings([FromQuery]string componentName)
    {
        var settings = new DataExpSettings(componentName);
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