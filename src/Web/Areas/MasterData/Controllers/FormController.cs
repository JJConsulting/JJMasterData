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
    private readonly FormViewFactory _formViewFactory;

    public FormController(FormViewFactory formViewFactory)
    {
        _formViewFactory = formViewFactory;
    }
    
    public IActionResult Render(string dictionaryName)
    {
        var model = new FormViewModel(dictionaryName, ConfigureFormView);
        return View(model);
    }

    
    [DictionaryNameDecryptionServiceFilter]
    [ActionMapDecryptionServiceFilter]
    [HttpPost]
    public async Task<IActionResult> GetFormView(
        string dictionaryName,
        PageState pageState,
        ActionMap actionMap)
    {
        var formView = await _formViewFactory.CreateFormViewAsync(dictionaryName);
        formView.IsModal = true;
        formView.IsExternalRoute = true;
        formView.PageState = pageState;

        if (pageState is not PageState.Insert)
        {
            formView.DataPanel.LoadValuesFromPK(actionMap.PkFieldValues);
        }

        var form = new HtmlBuilder(HtmlTag.Form);
        form.AppendElement(formView.GetHtmlBuilder());
        return Content(form.ToString());
    }
    
    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null) 
            return;
        
        formView.SetCurrentFilter("USERID", userId);
        formView.SetUserValues("USERID", userId);
    }



}