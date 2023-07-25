using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

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

    
    [ServiceFilter<FormElementDecryptionFilter>]
    [ServiceFilter<ActionMapDecryptionFilter>]
    [HttpPost]
    public async Task<IActionResult> GetFormView(
        FormElement formElement,
        PageState pageState,
        ActionMap actionMap)
    {
        var formView = _formViewFactory.CreateFormView(formElement);
        formView.IsModal = true;
        formView.IsExternalRoute = true;
        formView.PageState = pageState;

        if (pageState is not PageState.Insert)
        {
            await formView.DataPanel.LoadValuesFromPkAsync(actionMap.PkFieldValues);
        }

        var form = new HtmlBuilder(HtmlTag.Form);
        form.AppendElement(formView.GetHtmlBuilder());
        return Content(form.ToString());
    }
    
    [ServiceFilter<DictionaryNameDecryptionFilter>]
    [HttpPost]
    public async Task<IActionResult> ReloadPanel(
        string dictionaryName,
        string componentName)
    {
        var formView = await _formViewFactory.CreateFormViewAsync(dictionaryName);
        formView.Name = componentName;
        formView.IsExternalRoute = true;

        var html = await formView.GetReloadPanelHtmlAsync();
        
        return Content(html);
    }
    
    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null) 
            return;
        formView.IsExternalRoute = true;
        formView.GridView.SetCurrentFilter("USERID", userId);
        formView.SetUserValues("USERID", userId);
    }



}