using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController(IFormElementComponentFactory<JJFormView> formViewFactory) : MasterDataController
{
    public async Task<IActionResult> Render(string elementName)
    {
        var formView = await formViewFactory.CreateAsync(elementName);
        
        ConfigureFormView(formView);

        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;
        
        var model = new FormViewModel(formView.FormElement.Name, result.Content);
        return View(model);
    }
    
    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.User.GetUserId();

        if (userId == null) 
            return;
       
        formView.GridView.SetCurrentFilter("USERID", userId);

        formView.SetUserValues("USERID", userId);
    }
}