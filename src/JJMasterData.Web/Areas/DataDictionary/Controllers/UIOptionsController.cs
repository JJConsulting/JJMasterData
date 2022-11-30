using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class UIOptionsController : DataDictionaryController
{
    private readonly UIOptionsService? _optionsService;

    public UIOptionsController(UIOptionsService? optionsService)
    {
        _optionsService = optionsService;
    }

    public ActionResult Index(string dictionaryName)
    {
        return View(Populate(dictionaryName));
    }

    public ActionResult Edit(string dictionaryName)
    {
        return View(Populate(dictionaryName));
    }

    [HttpPost]
    public ActionResult Edit(UIOptions uIOptions, string dictionaryName)
    {
        if (_optionsService!.EditOptions(uIOptions, dictionaryName))
            return RedirectToAction("Index", new { dictionaryName });

        var jjValidationSummary = _optionsService.GetValidationSummary();
        ViewBag.Error = jjValidationSummary.GetHtml();
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Options";

        return View(uIOptions);
    }

    private UIOptions Populate(string dictionaryName)
    {
        var dicParser = _optionsService!.DictionaryRepository.GetMetadata(dictionaryName);
        var uIOptions = dicParser.UIOptions;
        ViewBag.MenuId = "Options";
        ViewBag.DictionaryName = dictionaryName;

        return uIOptions;
    }

}