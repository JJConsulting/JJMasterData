using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController : MasterDataController
{
    private readonly IFormElementComponentFactory<JJFormView> _formViewFactory;

    public FormController(IFormElementComponentFactory<JJFormView>  formViewFactory)
    {
        _formViewFactory = formViewFactory;
    }
    
    public async Task<IActionResult> Render(string dictionaryName)
    {
        var formView = await _formViewFactory.CreateAsync(dictionaryName);
        
        ConfigureFormView(formView);
        
        var result = await formView.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();
        
        var model = new FormViewModel(formView.FormElement.Title ?? formView.FormElement.Name, result.Content);
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
        var formView = _formViewFactory.Create(formElement);
        formView.IsModal = true;
        formView.IsExternalRoute = true;
        formView.PageState = pageState;

        if (pageState is not PageState.Insert)
        {
            await formView.DataPanel.LoadValuesFromPkAsync(actionMap.PkFieldValues);
        }

        var result = await formView.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();
        else if(result is RenderedComponentResult renderedComponentResult)
        {
            var form = new HtmlBuilder(HtmlTag.Form)
                .WithNameAndId(formView.Name)
                .WithAttribute("action",Url.Action("GetFormView")!)
                .WithAttribute("method", "POST");
            form.Append(renderedComponentResult.HtmlBuilder);
            return Content(form.ToString());
        }

        return new EmptyResult();
    }
    
    [ServiceFilter<FormElementDecryptionFilter>]
    [HttpPost]
    public async Task<IActionResult> ReloadPanel(
        FormElement formElement,
        PageState pageState,
        string fieldNamePrefix,
        string componentName)
    {
        var formView = _formViewFactory.Create(formElement);
        formView.Name = componentName;
        formView.DataPanel.FieldNamePrefix = fieldNamePrefix;
        formView.PageState = pageState;
        formView.IsExternalRoute = true;

        var html = await formView.GetReloadPanelResultAsync();
        
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