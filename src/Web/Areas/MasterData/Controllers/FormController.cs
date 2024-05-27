using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer
    ) : MasterDataController
{
    public async Task<IActionResult> Render(string elementName)
    {
        var formView = await formViewFactory.CreateAsync(elementName);
        
        ConfigureFormView(formView);

        var result = await formView.GetResultAsync();
        
        if (result is IActionResult actionResult)
            return actionResult;

        var model = new FormViewModel
        {
            FormTitle = formView.FormElement.Name,
            FormViewHtml = result.Content
        };
        
        return View(model);
    }
    
    private void ConfigureFormView(JJFormView formView)
    {
        var userId = HttpContext.User.GetUserId();

        if (userId == null) 
            return;
        
        if (HttpContext.User.HasClaim(c => c.Type is "DataDictionary"))
        {
            formView.TitleActions = [
                new TitleAction
                {
                    Url = Url.Action("Index","Entity", new {Area="DataDictionary", elementName=formView.FormElement.Name})!,
                    Icon = IconType.Pencil,
                    Tooltip = stringLocalizer["Edit Element"]
                }
            ];
        }
       
        formView.GridView.SetCurrentFilter("USERID", userId);

        formView.SetUserValues("USERID", userId);
    }
}