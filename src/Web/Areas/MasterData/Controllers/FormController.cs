﻿using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class FormController(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer) : MasterDataController
{
    [ViewData]
    public string? Title { get; set; }
   
    [ViewData]
    public string? FormViewHtml { get; set; }
    
    public async Task<IActionResult> Render(string elementName)
    {
        var formView = await formViewFactory.CreateAsync(elementName);

        ConfigureFormView(formView);

        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;

        SetTitle(formView.FormElement.Name);
        
        FormViewHtml = result.Content;

        HttpContext.Items["ElementName"] = elementName;

        return View();
    }

    private void SetTitle(string elementName)
    {
        if (TempData.TryGetValue("Title", out var title))
        {
            Title = title?.ToString();
        }
        else if (Request.HasFormContentType && Request.Form.TryGetValue("masterdata-title", out var mdTitle))
        {
            Title = mdTitle;
        }
        else
        {
            Title = elementName;
        }
    }

    private void ConfigureFormView(JJFormView formView)
    {
        var user = HttpContext.User;
        var userId = user.GetUserId();

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
                    Icon = IconType.Pencil,
                    Text = stringLocalizer["Edit Element"]
                }
            ];
        }

        formView.GridView.SetCurrentFilter("USERID", userId);

        formView.SetUserValues("USERID", userId);
    }
}