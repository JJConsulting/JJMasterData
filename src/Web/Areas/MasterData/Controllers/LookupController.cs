using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class LookupController : MasterDataController
{
    private LookupService LookupService { get; }
    private FormValuesService FormValuesService { get; }
    private IEncryptionService EncryptionService { get; }
    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public LookupController(
        LookupService lookupService,
        FormValuesService formValuesService,
        IEncryptionService encryptionService,
        IFormElementComponentFactory<JJFormView> formViewFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer
        )
    {
        LookupService = lookupService;
        FormValuesService = formValuesService;
        EncryptionService = encryptionService;
        FormViewFactory = formViewFactory;
        StringLocalizer = stringLocalizer;
    }

    [ServiceFilter<LookupParametersDecryptionFilter>]
    public async Task<IActionResult> Index(LookupParameters lookupParameters)
    {
        var formView = await FormViewFactory.CreateAsync(lookupParameters.ElementName);

        ConfigureLookupForm(formView, lookupParameters);
        
        var result = await formView.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();

        ViewBag.FormViewHtml = result.Content;

        var model = new LookupViewModel()
        {
            LookupFormViewHtml = result.Content,
            EncryptedLookupParameters = EncryptionService.EncryptStringWithUrlEscape(lookupParameters.ToQueryString())
        };
        
        return View(model);
    }

    [ServiceFilter<FormElementDecryptionFilter>]
    public async Task<IActionResult> GetDescription(
        FormElement formElement,
        string fieldName,
        string componentName,
        PageState pageState)
    {
        var elementMap = formElement.Fields[fieldName].DataItem!.ElementMap;
        var formValues = await FormValuesService.GetFormValuesWithMergedValuesAsync(formElement, pageState, true);
        var formStateData = new FormStateData(formValues, pageState);
        var selectedValue = LookupService.GetSelectedValue(componentName);
        
        var description = await LookupService.GetDescriptionAsync(elementMap!, formStateData, selectedValue, false);
        return Json(new LookupResultDto(selectedValue!, description!));
    }

    private void ConfigureLookupForm(JJFormView form, LookupParameters lookupParameters)
    {
        form.ShowTitle = false;

        if (!lookupParameters.EnableElementActions)
        {
            foreach (var action in form.GridView.ToolBarActions.Where(IsLookupAction()))
            {
                action.SetVisible(false);
            }

            foreach (var action in form.GridView.GridActions)
            {
                action.SetVisible(false);
            }
        }

        foreach (var action in form.GridView.GridActions)
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
        form.GridView.GridActions.Add(selAction);

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