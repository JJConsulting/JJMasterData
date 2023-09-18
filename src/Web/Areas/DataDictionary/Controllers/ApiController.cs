using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ApiController : DataDictionaryController
{
    private readonly ApiService _apiService;

    public ApiController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ActionResult> Index(string elementName)
    {
        var dic = await _apiService.DataDictionaryRepository.GetMetadataAsync(elementName);
        var model = PopulateViewModel(dic);

        return View(model);
    }

    public async Task<ActionResult> Edit(string elementName)
    {
        var dic = await _apiService.DataDictionaryRepository.GetMetadataAsync(elementName);
        var model = PopulateViewModel(dic);

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Edit(ApiViewModel apiViewModel)
    {
        var dic = await _apiService.DataDictionaryRepository.GetMetadataAsync( apiViewModel.ElementName);
        dic.ApiOptions = apiViewModel.MetadataApiOptions;
        dic.EnableApi = apiViewModel.IsSync;
        dic.SyncMode = apiViewModel.Mode;

        if (await _apiService.SetFormElementWithApiValidation(dic))
            return RedirectToAction("Index", new { elementName =  apiViewModel.ElementName });
        var model = PopulateViewModel(dic);
        model.ValidationSummary = _apiService.GetValidationSummary();
        return View(model);

    }
    private ApiViewModel PopulateViewModel(FormElement metadata)
    {
        var model = new ApiViewModel(elementName:metadata.Name, menuId:"Api")
        {
            MetadataApiOptions = metadata.ApiOptions,
            Mode = metadata.SyncMode,
            IsSync = metadata.EnableApi,
            Fields = new List<ElementField>(metadata.Fields.ToList().FindAll(
                x => (x.IsPk | x.Filter.Type != FilterMode.None) &
                     x.DataType != FieldType.DateTime &
                     x.DataType != FieldType.DateTime2 &
                     x.DataType != FieldType.Date
            ))
        };

        return model;
    }

}