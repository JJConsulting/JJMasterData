using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Areas.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class SettingsController(
    SettingsService service,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IComponentFactory componentFactory) : DataDictionaryController
{
    private SettingsService Service { get; } = service;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IComponentFactory ComponentFactory { get; } = componentFactory;

    public async Task<IActionResult> Index()
    {
        var viewModel = await Service.GetViewModel();
        
        return View(viewModel);
    }
    
    [HttpPost]
    public async Task<IActionResult> Save(SettingsViewModel model)
    {
        await Service.SaveOptions(model);

        await Task.Delay(250);
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> TestConnection(SettingsViewModel model)
    {
        var result = await SettingsService.GetConnectionResultAsync(model.ConnectionString.ToString(), model.CommonsOptions.DefaultConnectionProvider);
        var alert = ComponentFactory.Html.Alert.Create();
        if (!result.IsConnectionSuccessful.GetValueOrDefault())
        {
  
            alert.Title = StringLocalizer["Error while testing the connection."];
            alert.Icon = IconType.SolidXmark;
            alert.Color = BootstrapColor.Danger;
        }
        else
        {
            alert.Title = StringLocalizer["Connection successfully estabilished."];
            alert.Icon = IconType.CheckCircle;
            alert.Color = BootstrapColor.Success;
        }

        return new ContentResult
        {
            Content = alert.GetHtml()
        };
    }
}