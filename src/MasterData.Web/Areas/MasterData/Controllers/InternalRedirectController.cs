using System.Text.Json;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using Microsoft.AspNetCore.Http;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class InternalRedirectController(
    ExpressionsService expressionsService,
    IComponentFactory componentFactory, 
    FormService formService,
    IHttpContextAccessor request,
    IMasterDataUser masterDataUser,
    IEncryptionService encryptionService) : MasterDataController
{
    public async Task<IActionResult> Index(string parameters, string? multiselectValues)
    {
        var state = GetInternalRedirectState(parameters);
        var userId = masterDataUser.Id;
        var userValues = GetUserValues(userId, multiselectValues);
        
        InternalRedirectViewModel model;

        switch (state.RelationshipType)
        {
            case RelationshipViewType.List:
            {
                var formView = await componentFactory.FormView.CreateAsync(state.ElementName);
                formView.ShowTitle = state.ShowTitle;
                formView.RelationValues = state.RelationValues;
                formView.FormElement.Options.Grid.MaintainValuesOnLoad = false;
                ApplyUserValues(formView, userValues);
                
                if (userId != null)
                {
                    formView.GridView.SetCurrentFilter("USERID", userId);
                }
                
                var result = await formView.GetResultAsync();
                if (result is IActionResult actionResult)
                    return actionResult;

                var title = expressionsService.GetExpressionValue(formView.FormElement.Title, new FormStateData(state.RelationValues!, userValues, PageState.List))?.ToString();
                model = new()
                {
                    HtmlContent = result.HtmlContent,
                    ShowToolbar = false,
                    Title = title ?? formView.Name,
                    IsModal = state.OpenInModal,
                    ParentElementName = state.ParentElementName,
                    MultiselectValues = multiselectValues
                };
                break;
            }
            case RelationshipViewType.View:
            {
                var formView = await componentFactory.FormView.CreateAsync(state.ElementName);
                formView.PageState = PageState.View;
                ApplyUserValues(formView, userValues);

                await formView.DataPanel.LoadValuesFromPkAsync(state.RelationValues);
                
                DataHelper.CopyIntoDictionary(formView.DataPanel.Values, state.RelationValues!);
                
                var result = await formView.GetResultAsync();
                if (result is IActionResult actionResult)
                    return actionResult;
                
                var title = expressionsService.GetExpressionValue(formView.FormElement.Title, new FormStateData(state.RelationValues!, userValues, PageState.View))?.ToString();
                model = new()
                {
                    HtmlContent = result.HtmlContent,
                    ShowToolbar = false,
                    Title = title ?? formView.Name,
                    IsModal = state.OpenInModal,
                    ParentElementName = state.ParentElementName,
                    MultiselectValues = multiselectValues
                };
                break;
            }
            case RelationshipViewType.Insert:
            case RelationshipViewType.Update:
            {
                var formView = await componentFactory.FormView.CreateAsync(state.ElementName);
                var pageState = state.RelationshipType is RelationshipViewType.Update
                    ? PageState.Update
                    : PageState.Insert;
                
                formView.PageState = pageState;
                ApplyUserValues(formView, userValues);

                if (pageState is PageState.Update)
                {
                    await formView.DataPanel.LoadValuesFromPkAsync(state.RelationValues);
                }
                
                DataHelper.CopyIntoDictionary(formView.DataPanel.Values, state.RelationValues!);

                formView.FormElement.Options.FormToolbarActions.Clear();
                
                var result = await formView.GetResultAsync();
                if (result is IActionResult actionResult)
                    return actionResult;
                
                var title = expressionsService.GetExpressionValue(formView.FormElement.Title, new FormStateData(state.RelationValues!, userValues, pageState))?.ToString();
                model = new()
                {
                    HtmlContent = result.HtmlContent,
                    ShowToolbar = true,
                    Title = title ?? formView.Name,
                    IsModal = state.OpenInModal,
                    ParentElementName = state.ParentElementName,
                    MultiselectValues = multiselectValues
                };
                break;
            }
            default:
                throw new InvalidOperationException();
        }

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Save(string parameters, string? multiselectValues)
    {
        var state =  GetInternalRedirectState(parameters);
        var userId = masterDataUser.Id;
        var userValues = GetUserValues(userId, multiselectValues);
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
     
        ApplyUserValues(panel, userValues);

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

        var title = expressionsService.GetExpressionValue(panel.FormElement.Title, new FormStateData(state.RelationValues!, userValues, PageState.Update))?.ToString();
        
        if(!hasErrors && !state.OpenInModal)
            return RedirectToAction("Render","Form", new {Area="MasterData", elementName = state.ElementName});
        
        var model = new InternalRedirectViewModel
        {
            HtmlContent = result.HtmlContent,
            ShowToolbar = true,
            IsModal = state.OpenInModal,
            ParentElementName = state.ParentElementName,
            SubmitParentWindow = !hasErrors,
            Title = title ?? panel.Name,
            MultiselectValues = multiselectValues
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
                case "parentelementname":
                    state.ParentElementName = @params.Get(key)!;
                    break;
                case "openinmodal":
                    state.OpenInModal = @params.Get(key) == "1";
                    break;
                default:
                    state.RelationValues.Add(key, @params.Get(key)!);
                    break;
            }
        }
        
        return state;
    }

    private Dictionary<string, object?> GetUserValues(string? userId, string? encryptedMultiselectValues)
    {
        var userValues = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);

        if (!string.IsNullOrEmpty(encryptedMultiselectValues))
        {
            var multiselectValues =  GetMultiselectValues(encryptedMultiselectValues);
            userValues["MultiselectValues"] = multiselectValues;
        }
        
        if (userId != null)
            userValues["USERID"] = userId;

        return userValues;
    }

    private static void ApplyUserValues(JJFormView formView, Dictionary<string, object?> userValues)
    {
        foreach (var kvp in userValues)
        {
            if (kvp.Value is string stringValue)
                formView.SetUserValues(kvp.Key, stringValue);
        }
    }

    private static void ApplyUserValues(JJDataPanel panel, Dictionary<string, object?> userValues)
    {
        foreach (var kvp in userValues)
        {
            if (kvp.Value is string stringValue)
                panel.SetUserValues(kvp.Key, stringValue);
        }
    }

    private string? GetMultiselectValues(string? selectedRows)
    {
        if (string.IsNullOrWhiteSpace(selectedRows))
            return null;
        
        var selectedValues = new HashSet<string>();

        foreach (var encryptedPk in selectedRows.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var decryptedPk = encryptionService.DecryptStringWithUrlUnescape(encryptedPk);
            selectedValues.Add(decryptedPk);
        }

        return string.Join(',', selectedValues);
    }
}
