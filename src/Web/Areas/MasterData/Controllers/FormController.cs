using System.Collections;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController : MasterDataController
{
    private readonly JJMasterDataFactory _masterDataFactory;

    public FormController(JJMasterDataFactory masterDataFactory)
    {
        _masterDataFactory = masterDataFactory;
    }
    
    public IActionResult Render(string dictionaryName)
    {
        var model = new FormViewModel(dictionaryName, ConfigureFormView);
        return View(model);
    }

    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null) 
            return;
        
        formView.SetCurrentFilter("USERID", userId);
        formView.SetUserValues("USERID", userId);
    }

    [HttpPost]
    [DictionaryNameDecryptionServiceFilter]
    public IActionResult GetGridViewTable(string dictionaryName)
    {
        var formView = new JJFormView(dictionaryName);
        return Content(formView.GetTableHtml());
    }

    
    [DictionaryNameDecryptionServiceFilter]
    [ActionMapDecryptionServiceFilter]
    [HttpPost]
    public IActionResult GetFormView(
        string dictionaryName,
        PageState pageState,
        ActionMap actionMap
        )
    {
        var formView = new JJFormView(dictionaryName)
        {
            IsExternalRoute = true,
            PageState = pageState,
            IsModal = true,
        };

        if (pageState is not PageState.Insert)
        {
            formView.DataPanel.LoadValuesFromPK(actionMap.PkFieldValues);
        }

        var form = new HtmlBuilder(HtmlTag.Form);
        form.AppendElement(formView.GetHtmlBuilder());
        return Content(form.ToString());
    }
    
    public IActionResult ExportUI()
    {
        throw new NotImplementedException();
    }
    

    [HttpPost]
    public IActionResult SearchValues(string dictionaryName, string fieldName, int pageState)
    {
        var searchBox = _masterDataFactory.CreateJJSearchBox(dictionaryName, fieldName, (PageState)pageState, null);
        return Json(searchBox.GetListBoxItems());
    }


}