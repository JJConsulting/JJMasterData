using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController : MasterDataController
{
    private readonly IFormElementComponentFactory<JJFormView> _formViewFactory;

    public FormController(IFormElementComponentFactory<JJFormView> formViewFactory)
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
    [HttpPost]
    public async Task<IActionResult> ReloadPanel(
        FormElement formElement,
        PageState pageState,
        string componentName)
    {
        var formView = _formViewFactory.Create(formElement);
        formView.Name = componentName;
        formView.DataPanel.FieldNamePrefix = componentName + "_";
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