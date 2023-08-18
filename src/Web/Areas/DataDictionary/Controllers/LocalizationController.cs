using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.Options;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

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