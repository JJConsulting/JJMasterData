using System.Reflection;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.WebComponents;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Models;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class SettingsController : MasterDataController
{
    private SettingsService Service { get; }
    
    public SettingsController(SettingsService service)
    {
        Service = service;
    }
    
    public IActionResult About()
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var model = new AboutViewModel
        {
            ExecutingAssemblyProduct = Service.GetAssemblyProduct(executingAssembly),
            ExecutingAssemblyVersion =executingAssembly.GetName().Version?.ToString(),
            ExecutingAssemblyCopyright = Service.GetAssemblyCopyright(executingAssembly),
            BootstrapVersion = BootstrapHelper.Version.ToString(),
            Dependencies = Service.GetJJAssemblies()
        };

        return View("About", model);
    }

    public IActionResult Index()
    {
        var settings = new SettingsViewModel
        {
            ConnectionString = new ConnectionString(Service.Options.GetConnectionString("ConnectionString")),
            BootstrapVersion = Service.Options.BootstrapVersion
        };
        return View(settings);
    }
    
    public async Task<IActionResult> Save(SettingsViewModel model)
    {
        if (ModelState.IsValid)
        {
            
        }
        
        return View(nameof(Index), model);
    }
    
    public async Task<IActionResult> TestConnection(SettingsViewModel model)
    {
        if (ModelState.IsValid)
        {
            string connectionString = model.ConnectionString.ToString();

            var result = await Service.TryConnectionAsync(connectionString);
        
            model.ConnectionString.ConnectionResult = new ConnectionResult(result.Item1,result.Item2);
        }
        
        return View(nameof(Index), model);
    }
}