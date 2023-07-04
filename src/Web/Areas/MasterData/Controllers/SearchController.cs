using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

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
    [DictionaryNameDecryptionServiceFilter]
    public async Task<IActionResult> GetItems(
        string dictionaryName,
        string fieldName,
        string fieldSearchName,
        int pageState
    )
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        var searchText = HttpContext.Request.Form[fieldSearchName];
        var dataItem = formElement.Fields[fieldName].DataItem;

        IDictionary<string, dynamic>? formValues = null;

        if (dataItem!.HasSqlExpression())
        {
            formValues = await FormValuesService.GetFormValuesWithMergedValues(formElement, (PageState)pageState, true);
        }

        var context = new SearchBoxContext(formValues, null, (PageState)pageState);

        var values = await Service.GetValues(dataItem,searchText,null,context);
        var items = Service.GetItems(dataItem,values);
        
        return Json(items);
    }
}