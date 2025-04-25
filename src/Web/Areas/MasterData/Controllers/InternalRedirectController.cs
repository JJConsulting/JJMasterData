using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class InternalRedirectController(
    ExpressionsService expressionsService,
    IComponentFactory componentFactory, 
    FormService formService,
    IHttpRequest request,
    IEncryptionService encryptionService) : MasterDataController
{
    public async Task<IActionResult> Index(string parameters)
    {
        var state = GetInternalRedirectState(parameters);
        var userId = HttpContext.User.GetUserId();
        InternalRedirectViewModel model;

        switch (state.RelationshipType)
        {
            case RelationshipViewType.List:
            {
                var formView = await componentFactory.FormView.CreateAsync(state.ElementName);
                formView.ShowTitle = state.ShowTitle;
                formView.RelationValues = state.RelationValues;
                formView.FormElement.Options.Grid.MaintainValuesOnLoad = false;
                
                if (userId != null)
                {
                    formView.SetUserValues("USERID", userId);
                    formView.GridView.SetCurrentFilter("USERID", userId);
                }
                
                var result = await formView.GetResultAsync();
                if (result is IActionResult actionResult)
                    return actionResult;

                var title = expressionsService.GetExpressionValue(formView.FormElement.Title, new FormStateData(state.RelationValues!, PageState.List))?.ToString();
                model = new()
                {
                    HtmlContent = result.Content,
                    ShowToolbar = false,
                    Title = title ?? formView.Name
                };
                break;
            }
            case RelationshipViewType.View:
            {
                var panel = await componentFactory.DataPanel.CreateAsync(state.ElementName);
                panel.PageState = PageState.View;
                
                if (userId != null)
                    panel.SetUserValues("USERID", userId);

                await panel.LoadValuesFromPkAsync(state.RelationValues);
                
                DataHelper.CopyIntoDictionary(panel.Values, state.RelationValues!);
                
                var result = await panel.GetResultAsync();
                if (result is IActionResult actionResult)
                    return actionResult;
                
                var title = expressionsService.GetExpressionValue(panel.FormElement.Title, new FormStateData(state.RelationValues!, PageState.View))?.ToString();
                model = new()
                {
                    HtmlContent =  result.Content,
                    ShowToolbar = false,
                    Title = title ?? panel.Name
                };
                break;
            }
            case RelationshipViewType.Insert:
            case RelationshipViewType.Update:
            {
                var panel = await componentFactory.DataPanel.CreateAsync(state.ElementName);

                var pageState = state.RelationshipType is RelationshipViewType.Update
                    ? PageState.Update
                    : PageState.Insert;
                
                panel.PageState = pageState;

                if (pageState is PageState.Update)
                {
                    await panel.LoadValuesFromPkAsync(state.RelationValues);
                }
                
                DataHelper.CopyIntoDictionary(panel.Values, state.RelationValues!);
                
                if (userId != null)
                    panel.SetUserValues("USERID", userId);
                
                var result = await panel.GetResultAsync();
                if (result is IActionResult actionResult)
                    return actionResult;
                
                var title = expressionsService.GetExpressionValue(panel.FormElement.Title, new FormStateData(state.RelationValues!,pageState))?.ToString();
                model = new()
                {
                    HtmlContent = result.Content,
                    ShowToolbar = true,
                    Title = title ?? panel.Name
                };
                break;
            }
            default:
                throw new InvalidOperationException();
        }

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Save(string parameters)
    {
        var state = GetInternalRedirectState(parameters);
        var userId = HttpContext.User.GetUserId();
        var panel = await componentFactory.DataPanel.CreateAsync(state.ElementName);

        if (panel.PageState is PageState.Update)
        {
            await panel.LoadValuesFromPkAsync(state.RelationValues);
        }
        else
        {
            foreach (var kvp in state.RelationValues)
            {
                panel.Values[kvp.Key] = kvp.Value;
            }
        }
     
        if (userId != null)
            panel.SetUserValues("USERID", userId);

        var values = await panel.GetFormValuesAsync();
        var letter = await formService.InsertOrReplaceAsync(panel.FormElement, values, new DataContext(request, DataContextSource.Form, userId));
        
        var hasErrors = letter.Errors.Count > 0;
        if (hasErrors)
        {
            foreach (var error in letter.Errors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }
        }

        var result = await panel.GetResultAsync();
        if (result is IActionResult actionResult)
            return actionResult;

        var title = expressionsService.GetExpressionValue(panel.FormElement.Title, new FormStateData(state.RelationValues!, PageState.Update))?.ToString();
        var model = new InternalRedirectViewModel
        {
            HtmlContent = result.Content,
            ShowToolbar = false,
            SubmitParentWindow = !hasErrors,
            Title = title ?? panel.Name
        };

        return View("Index", model);
    }
    
    private InternalRedirectState GetInternalRedirectState(string parameters)
    {
        if (string.IsNullOrEmpty(parameters))
            throw new ArgumentNullException(nameof(parameters));

        var state = new InternalRedirectState
        {
            RelationshipType = RelationshipViewType.List
        };

        var @params = HttpUtility.ParseQueryString(encryptionService.DecryptStringWithUrlUnescape(parameters));
        state.ElementName = @params.Get("formname");

        foreach (string key in @params)
        {
            switch (key.ToLower())
            {
                case "formname":
                    state.ElementName = @params.Get(key);
                    break;
                case "showtitle":
                    state.ShowTitle = @params.Get(key) == "1";
                    break;
                case "viewtype":
                    state.RelationshipType = (RelationshipViewType)int.Parse(@params.Get(key) ?? string.Empty);
                    break;
                default:
                    state.RelationValues.Add(key, @params.Get(key)!);
                    break;
            }
        }

        return state;
    }
}