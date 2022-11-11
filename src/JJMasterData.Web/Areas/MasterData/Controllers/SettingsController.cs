using JJMasterData.Web.Controllers;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class SettingsController : DataDictionaryController
{
    private SettingsService Service { get; }
    
    public SettingsController(SettingsService service)
    {
        Service = service;
    }

    public IActionResult Index()
    {
        var dataPanel = Service.GetDataPanel();
        return View(dataPanel);
    }
}