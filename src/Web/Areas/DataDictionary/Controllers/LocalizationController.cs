using JJMasterData.Web.Areas.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LocalizationController(LocalizationService localizationService) : DataDictionaryController
{
    private LocalizationService LocalizationService { get; } = localizationService;

    public async Task<IActionResult> Index()
    {
        var formView = LocalizationService.GetFormView();

        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;
        
        return View(nameof(Index),result.Content!);
    }

}