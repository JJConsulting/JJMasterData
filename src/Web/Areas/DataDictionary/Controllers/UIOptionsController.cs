using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class UIOptionsController : DataDictionaryController
{
    private readonly UIOptionsService? _optionsService;

    public UIOptionsController(UIOptionsService? optionsService)
    {
        _optionsService = optionsService;
    }

    public async Task<ActionResult> Index(string dictionaryName)
    {
        return View(await Populate(dictionaryName));
    }

    public async Task<ActionResult> Edit(string dictionaryName)
    {
        return View(await Populate(dictionaryName));
    }

    [HttpPost]
    public async Task<ActionResult> Edit(FormElementOptions uIMetadataOptions, string dictionaryName)
    {
        if (await _optionsService!.EditOptionsAsync(uIMetadataOptions, dictionaryName))
            return RedirectToAction("Index", new { dictionaryName });

        var jjValidationSummary = _optionsService.GetValidationSummary();
        ViewBag.Error = jjValidationSummary.GetHtml();
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Options";

        return View(uIMetadataOptions);
    }

    private async Task<FormElementOptions> Populate(string dictionaryName)
    {
        var dicParser = await _optionsService!.DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        var uIOptions = dicParser.Options;
        ViewBag.MenuId = "Options";
        ViewBag.DictionaryName = dictionaryName;

        return uIOptions;
    }

}