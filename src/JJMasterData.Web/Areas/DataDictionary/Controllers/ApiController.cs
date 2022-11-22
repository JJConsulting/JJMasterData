using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class ApiController : DataDictionaryController
{
    private readonly ApiService _apiService;

    public ApiController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public ActionResult Index(string dictionaryName)
    {
        var dic = _apiService.DictionaryRepository.GetDictionary(dictionaryName);
        var model = PopulateViewModel(dic);

        return View(model);
    }

    public ActionResult Edit(string dictionaryName)
    {
        var dic = _apiService.DictionaryRepository.GetDictionary(dictionaryName);
        var model = PopulateViewModel(dic);

        return View(model);
    }

    [HttpPost]
    public ActionResult Edit(ApiViewModel apiViewModel)
    {
        var dic = _apiService.DictionaryRepository.GetDictionary( apiViewModel.DictionaryName);
        dic.Api = apiViewModel.ApiSettings;
        dic.Table.Sync = apiViewModel.IsSync;
        dic.Table.SyncMode = apiViewModel.Mode;

        if (_apiService.EditApi(dic))
            return RedirectToAction("Index", new { dictionaryName =  apiViewModel.DictionaryName });
        var model = PopulateViewModel(dic);
        model.ValidationSummary = _apiService.GetValidationSummary();
        return View(model);

    }
    private ApiViewModel PopulateViewModel(Dictionary dic)
    {
        var model = new ApiViewModel
        {
            ApiSettings = dic.Api,
            MenuId = "Api",
            DictionaryName = dic.Table.Name,
            Mode = dic.Table.SyncMode,
            IsSync = dic.Table.Sync,
            Fields = dic.Table.Fields.ToList().FindAll(
                x => (x.IsPk | x.Filter.Type != FilterMode.None) &
                     x.DataType != FieldType.DateTime &
                     x.DataType != FieldType.Date
            )
        };

        return model;
    }

}