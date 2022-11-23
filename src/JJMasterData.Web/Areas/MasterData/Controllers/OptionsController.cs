using JJMasterData.Commons.Dao;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class OptionsController : MasterDataController
{
    private OptionsService Service { get; }
    
    public OptionsController(OptionsService service)
    {
        Service = service;
    }
    
    public async Task<IActionResult> Index(bool isFullscreen=false)
    {
        var settings = new OptionsViewModel
        {
            ConnectionString = new ConnectionString(Service.Options.GetConnectionString()),
            Options = Service.Options,
            ConnectionProvider = DataAccessProvider.GetDataAccessProviderTypeFromString(Service.Options.GetConnectionProvider()),
            FilePath = Service.JJMasterDataOptionsWriter.FilePath,
            IsFullscreen = isFullscreen,
        };

        if (isFullscreen)
        {
            settings.ConnectionString.ConnectionResult = 
                await GetConnectionResultAsync(settings.ConnectionString.ToString());
        }

        return View(settings);
    }
    
    public async Task<IActionResult> Save(OptionsViewModel model)
    {
        if (ModelState.IsValid)
        {
            await Service.SaveOptions(model);
            ViewBag.Success = true;
            if (model.IsFullscreen)
                return RedirectToAction("Index","Element", new {Area="DataDictionary"});
        }
        
        return View(nameof(Index), model);
    }
    
    public async Task<IActionResult> TestConnection(OptionsViewModel model)
    {
        if (ModelState.IsValid)
        {
            model.ConnectionString.ConnectionResult = await GetConnectionResultAsync(model.ConnectionString.ToString());
        }

        return View(nameof(Index), model);
    }

    private async Task<ConnectionResult> GetConnectionResultAsync(string connectionString)
    {
        var result = await Service.TryConnectionAsync(connectionString);
        return new ConnectionResult(result.Item1, result.Item2);
    }
}