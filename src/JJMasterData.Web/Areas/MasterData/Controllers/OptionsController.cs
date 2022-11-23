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
public class OptionsController : MasterDataController
{
    private OptionsService Service { get; }
    
    public OptionsController(OptionsService service)
    {
        Service = service;
    }
    


    public IActionResult Index(bool isFullscreen=false)
    {
        var settings = new OptionsViewModel
        {
            ConnectionString = new ConnectionString(Service.Options.GetConnectionString("ConnectionString")),
            Options = Service.Options,
            IsFullscreen = isFullscreen
        };
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
            string connectionString = model.ConnectionString.ToString();
            var result = await Service.TryConnectionAsync(connectionString);
            model.ConnectionString.ConnectionResult = new ConnectionResult(result.Item1,result.Item2);
        }

        return View(nameof(Index), model);
    }
}