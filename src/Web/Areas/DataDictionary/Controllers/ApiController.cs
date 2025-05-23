﻿using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ApiController(ApiService apiService) : DataDictionaryController
{
    public async Task<ActionResult> Index(string elementName)
    {
        var dic = await apiService.GetFormElementAsync(elementName);
        var model = PopulateViewModel(dic);

        return View(model);
    }

    public async Task<ActionResult> Edit(string elementName)
    {
        var dic = await apiService.GetFormElementAsync(elementName);
        var model = PopulateViewModel(dic);

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Edit(ApiViewModel apiViewModel)
    {
        var formElement = await apiService.GetFormElementAsync( apiViewModel.ElementName);
        formElement.ApiOptions = apiViewModel.ApiOptions;
        formElement.EnableSynchronism = apiViewModel.EnableSynchronism;
        formElement.SynchronismMode = apiViewModel.SynchronismMode;

        if (await apiService.SetFormElementWithApiValidation(formElement))
            return RedirectToAction("Index", new { elementName =  apiViewModel.ElementName });
        
        var model = PopulateViewModel(formElement);

        return View(model);

    }
    private static ApiViewModel PopulateViewModel(FormElement metadata)
    {
        var model = new ApiViewModel
        {
            ElementName = metadata.Name,
            ApiOptions = metadata.ApiOptions,
            SynchronismMode = metadata.SynchronismMode,
            EnableSynchronism = metadata.EnableSynchronism,
            ElementFields =
            [
                ..metadata.Fields.FindAll(
                    x => (x.IsPk || x.Filter.Type != FilterMode.None) &&
                         x.DataType != FieldType.DateTime &&
                         x.DataType != FieldType.DateTime2 &&
                         x.DataType != FieldType.Date
                )
            ]
        };

        return model;
    }

}