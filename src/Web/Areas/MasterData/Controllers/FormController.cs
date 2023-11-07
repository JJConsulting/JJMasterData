using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController : MasterDataController
{
    private readonly IFormElementComponentFactory<JJFormView> _formViewFactory;

    public FormController(IFormElementComponentFactory<JJFormView> formViewFactory)
    {
        _formViewFactory = formViewFactory;
    }
    
    public async Task<IActionResult> Render(string elementName)
    {
        var formView = await _formViewFactory.CreateAsync(elementName);
        
        ConfigureFormView(formView);

        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;
        
        var model = new FormViewModel(formView.FormElement.Name, result.Content);
        return View(model);
    }
    
    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.GetUserId();

        if (userId == null) 
            return;
        
        formView.GridView.SetCurrentFilter("USERID", userId);
        formView.SetUserValues("USERID", userId);
    }
}