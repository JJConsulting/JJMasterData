using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.MasterData.Models;
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
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    [ServiceFilter<LookupParametersDecryptionFilter>]
    public async Task<IActionResult> Index(LookupParameters lookupParameters)
    {
        var formView = await FormViewFactory.CreateAsync(lookupParameters.ElementName);

        ConfigureLookupForm(formView, lookupParameters);
        
        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;

        ViewBag.FormViewHtml = result.Content;

        var model = new LookupViewModel()
        {
            LookupFormViewHtml = result.Content,
            EncryptedLookupParameters = EncryptionService.EncryptStringWithUrlEscape(lookupParameters.ToQueryString())
        };
        
        return View(model);
    }

    private void ConfigureLookupForm(JJFormView form, LookupParameters lookupParameters)
    {
        form.ShowTitle = false;

        if (!lookupParameters.EnableElementActions)
        {
            foreach (var action in form.GridView.ToolbarActions.Where(IsLookupAction()))
            {
                action.SetVisible(false);
            }

            foreach (var action in form.GridView.GridTableActions)
            {
                action.SetVisible(false);
            }
        }

        foreach (var action in form.GridView.GridTableActions)
        {
            action.IsDefaultOption = false;
        }

        var script = $"LookupHelper.setLookupValues('{lookupParameters.ComponentName}','{{{lookupParameters.FieldKeyName}}}','{{{lookupParameters.FieldValueName}}}');";
        var selAction = new ScriptAction
        {
            Name = "jjselLookup",
            Icon = IconType.ChevronRight,
            Tooltip = StringLocalizer["Select"],
            OnClientClick = script,
            IsDefaultOption = true,
            Order = 100
        };
        form.GridView.GridTableActions.Add(selAction);

        foreach (var filter in lookupParameters.Filters)
        {
            form.GridView.SetCurrentFilter(filter.Key, filter.Value!.ToString()!);
        }
    }

    private static Func<BasicAction, bool> IsLookupAction()
    {
        return action => action is not LegendAction
                         && action is not RefreshAction
                         && action is not FilterAction
                         && action is not ConfigAction
                         && action is not SortAction;
    }
}