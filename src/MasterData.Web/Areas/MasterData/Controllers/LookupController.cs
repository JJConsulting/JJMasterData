using JJConsulting.FontAwesome;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class LookupController(
        IEncryptionService encryptionService,
        IFormElementComponentFactory<JJFormView> formViewFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : MasterDataController
{
    [ViewData]
    public string? LookupFormViewHtml { get; set; } 
    
    [ViewData]
    public string? EncryptedLookupParameters { get; set; }
    
    [ServiceFilter<LookupParametersDecryptionFilter>]
    public async Task<IActionResult> Index(LookupParameters lookupParameters)
    {
        var formView = await formViewFactory.CreateAsync(lookupParameters.ElementName);

        ConfigureLookupForm(formView, lookupParameters);
        
        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;
        
        LookupFormViewHtml = result.Content;
        EncryptedLookupParameters = encryptionService.EncryptStringWithUrlEscape(lookupParameters.ToQueryString());
        
        return View();
    }

    private void ConfigureLookupForm(JJFormView form, LookupParameters lookupParameters)
    {
        form.ShowTitle = false;

        if (!lookupParameters.EnableElementActions)
        {
            foreach (var action in form.GridView.ToolbarActions.Where(IsLookupAction))
            {
                action.SetVisible(false);
            }

            foreach (var action in form.GridView.TableActions)
            {
                action.SetVisible(false);
            }
        }

        foreach (var action in form.GridView.TableActions)
        {
            action.IsDefaultOption = false;
        }

        var script = $"LookupHelper.setLookupValues('{lookupParameters.ComponentName}','{{{lookupParameters.FieldKeyName}}}','{{{lookupParameters.FieldValueName}}}');";
        var selAction = new ScriptAction
        {
            Name = "jjselLookup",
            Icon = FontAwesomeIcon.ChevronRight,
            Tooltip = stringLocalizer["Select"],
            OnClientClick = script,
            IsDefaultOption = true,
            Order = 100
        };
        form.GridView.TableActions.Add(selAction);

        foreach (var filter in lookupParameters.Filters)
        {
            form.GridView.SetCurrentFilter(filter.Key, filter.Value!.ToString()!);
        }
    }

    private static bool IsLookupAction(BasicAction action)
    {
        return action is not LegendAction
                         && action is not RefreshAction
                         && action is not FilterAction
                         && action is not ConfigAction
                         && action is not SortAction;
    }
}