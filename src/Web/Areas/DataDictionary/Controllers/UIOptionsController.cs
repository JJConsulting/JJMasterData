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

    public async Task<ActionResult> Index(string elementName)
    {
        return View(await Populate(elementName));
    }

    public async Task<ActionResult> Edit(string elementName)
    {
        return View(await Populate(elementName));
    }

    [HttpPost]
    public async Task<ActionResult> Edit(FormElementOptions uIMetadataOptions, string elementName)
    {
        if (await _optionsService!.EditOptionsAsync(uIMetadataOptions, elementName))
            return RedirectToAction("Index", new { elementName });

        var jjValidationSummary = _optionsService.GetValidationSummary();
        ViewBag.Error = jjValidationSummary.GetHtml();
        ViewBag.ElementName = elementName;
        ViewBag.MenuId = "Options";

        return View(uIMetadataOptions);
    }

    private async Task<FormElementOptions> Populate(string elementName)
    {
        var dicParser = await _optionsService!.DataDictionaryRepository.GetMetadataAsync(elementName);
        var uIOptions = dicParser.Options;
        ViewBag.MenuId = "Options";
        ViewBag.ElementName = elementName;

        return uIOptions;
    }

}