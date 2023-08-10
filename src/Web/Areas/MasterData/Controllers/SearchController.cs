using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class SearchController : MasterDataController
{
    private IDataItemService Service { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IFormValuesService FormValuesService { get; }
    private IExpressionsService ExpressionsService { get; }

    public SearchController(
        IDataItemService service,
        IDataDictionaryRepository dataDictionaryRepository,
        IFormValuesService formValuesService,
        IExpressionsService expressionsService)
    {
        Service = service;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
    }
    
    [HttpPost]
    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetItems(
        FormElement formElement,
        string fieldName,
        string fieldSearchName,
        int pageState
    )
    {
        var searchText = HttpContext.Request.Form[fieldSearchName];
        var dataItem = formElement.Fields[fieldName].DataItem;

        IDictionary<string, dynamic>? formValues;
        if (dataItem!.HasSqlExpression())
            formValues = await FormValuesService.GetFormValuesWithMergedValuesAsync(formElement, (PageState)pageState, true);
        else
            formValues = new Dictionary<string, dynamic>();

        var formStateData = new FormStateData(formValues, (PageState)pageState);
        var listValues = await Service.GetValuesAsync(dataItem, formStateData, searchText, null).ToListAsync();
        var items = Service.GetItems(dataItem, listValues);
        
        return Json(items);
    }
}