using System.Globalization;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Areas.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LocalizationController(LocalizationService localizationService) : DataDictionaryController
{
    public async Task<IActionResult> Index([FromQuery] bool isModal)
    {
        var formView = localizationService.GetFormView();
        formView.ShowTitle = !isModal;
        
        var result = await formView.GetResultAsync();
        
        if (result is IActionResult actionResult)
            return actionResult;
        
        return View(new LocalizationViewModel
        {
            FormViewHtml = result.Content,
            IsModal = isModal
        });
    }

    public async Task<FileContentResult> DownloadStrings()
    {
        var fileBytes = await localizationService.GetAllStringsFile();
        return File(fileBytes,"application/octet-stream",$"LocalizationStrings-{CultureInfo.CurrentUICulture.Name}.csv");
    }
}