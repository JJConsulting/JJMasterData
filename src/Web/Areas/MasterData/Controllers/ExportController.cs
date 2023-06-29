using JJMasterData.Core.DataManager.Exports.Configuration;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class ExportController : MasterDataController
{
    private IHttpContext MasterDataHttpContext { get; }

    public ExportController(IHttpContext httpContext)
    {
        MasterDataHttpContext = httpContext;
    }
    
    [HttpGet]
    public IActionResult Settings([FromQuery]string componentName)
    {
        var settings = new DataExpSettings(componentName, ExportOptions.LoadFromForm(MasterDataHttpContext, componentName));
        return Content(settings.GetHtmlElement().ToString());
    }

}