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
    public ActionResult Edit(MetadataOptions uIMetadataOptions, string dictionaryName)
    {
        if (_optionsService!.EditOptions(uIMetadataOptions, dictionaryName))
            return RedirectToAction("Index", new { dictionaryName });

        var jjValidationSummary = _optionsService.GetValidationSummary();
        ViewBag.Error = jjValidationSummary.GetHtml();
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Options";

        return View(uIMetadataOptions);
    }

    private MetadataOptions Populate(string dictionaryName)
    {
        var dicParser = _optionsService!.DataDictionaryRepository.GetMetadata(dictionaryName);
        var uIOptions = dicParser.MetadataOptions;
        ViewBag.MenuId = "Options";
        ViewBag.DictionaryName = dictionaryName;

        return uIOptions;
    }

}