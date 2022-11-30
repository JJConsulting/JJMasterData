using JJMasterData.Web.Areas.MasterData.Models;
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
        var viewModel = await Service.GetViewModel(isFullscreen);
        
        return View(viewModel);
    }
    

    public async Task<IActionResult> Save(OptionsViewModel model)
    {

        await Service.SaveOptions(model);

        if (!ModelState.IsValid)
        {
            model.ValidationSummary = Service.GetValidationSummary();
        }
        
        return View(nameof(Index), model);
    }
    
    public async Task<IActionResult> TestConnection(OptionsViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await OptionsService.GetConnectionResultAsync(model.ConnectionString.ToString());
            model.IsConnectionSuccessful = result.IsConnectionSuccessful;

            if (!result.IsConnectionSuccessful.GetValueOrDefault())
            {
                ModelState.AddModelError(nameof(OptionsViewModel.ConnectionString),result.ErrorMessage!);
            }

            model.ValidationSummary = Service.GetValidationSummary();
        }

        return View(nameof(Index), model);
    }
}