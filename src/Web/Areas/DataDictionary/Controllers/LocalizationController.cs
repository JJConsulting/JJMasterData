using JJMasterData.Web.Areas.DataDictionary.Services;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LocalizationController : DataDictionaryController
{
    private LocalizationService LocalizationService { get; }
    public LocalizationController(LocalizationService localizationService)
    {
        LocalizationService = localizationService;
    }

    public async Task<IActionResult> Index()
    {
        var formView = LocalizationService.GetFormView();

        var result = await formView.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();
        
        return View(nameof(Index),result.Content!);
    }

}