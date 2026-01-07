using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    IMasterDataUser masterDataUser,
    IStringLocalizer<MasterDataResources> stringLocalizer) : MasterDataController
{
    public async Task<IActionResult> Render(string elementName)
    {
        var formView = await formViewFactory.CreateAsync(elementName);

        ConfigureFormView(formView);

        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;

        SetTitle(formView.FormElement.Name);
        
        ViewData["FormViewHtml"] = result.Content;

        HttpContext.Items["ElementName"] = elementName;

        return View();
    }

    private void SetTitle(string elementName)
    {
        if (TempData.TryGetValue("Title", out var title))
        {
            ViewData["Title"] = title?.ToString();
        }
        else if (Request.HasFormContentType && Request.Form.TryGetValue("masterdata-title", out var mdTitle))
        {
            ViewData["Title"] = mdTitle;
        }
        else
        {
            ViewData["Title"] = elementName;
        }
    }

    private void ConfigureFormView(JJFormView formView)
    {
        var user = HttpContext.User;
        var userId = masterDataUser.Id;

        if (userId == null)
            return;

        if (user.IsInRole("Admin") || user.HasClaim(c => c.Type is "DataDictionary"))
        {
            formView.TitleActions =
            [
                new TitleAction
                {
                    Url = Url.Action("Index", "Entity",
                        new { Area = "DataDictionary", elementName = formView.FormElement.Name })!,
                    Icon = FontAwesomeIcon.Pencil,
                    Text = stringLocalizer["Edit Element"]
                }
            ];
        }

        formView.GridView.SetCurrentFilter("USERID", userId);
        formView.SetUserValues("USERID", userId);
    }
}