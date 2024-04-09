using System.Globalization;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Areas.DataDictionary.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LocalizationController(LocalizationService localizationService) : DataDictionaryController
{
    private LocalizationService LocalizationService { get; } = localizationService;

    public async Task<IActionResult> Index(bool isModal)
    {
        var formView = LocalizationService.GetFormView();
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
        var fileBytes = await LocalizationService.GetAllStringsFile();
        return File(fileBytes,"application/octet-stream",$"LocalizationStrings-{CultureInfo.CurrentUICulture.Name}.csv");
    }
    
}