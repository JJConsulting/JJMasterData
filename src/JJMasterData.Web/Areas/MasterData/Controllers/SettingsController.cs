using JJMasterData.Web.Controllers;
using JJMasterData.Web.Models;
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
        var settings = new SettingsViewModel
        {
            ConnectionString = new ConnectionString(Service.Settings.ConnectionString),
            BootstrapVersion = Service.Settings.BootstrapVersion
        };
        return View(settings);
    }
}