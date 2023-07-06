using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class LookupController : MasterDataController
{
    private FormViewFactory FormViewFactory { get; }

    public LookupController(FormViewFactory formViewFactory)
    {
        FormViewFactory = formViewFactory;
    }
    
    [ServiceFilter<LookupParametersDecryptionFilter>]
    public IActionResult Index(LookupParameters lookupParameters)
    {
        return View(new LookupViewModel
        {
            LookupParameters = lookupParameters,
            LookupFormConfiguration = ConfigureLookupForm
        }); 
    }

    private static void ConfigureLookupForm(JJFormView form, LookupParameters lookupParameters)
    {
        form.ShowTitle = false;

        if (!lookupParameters.EnableElementActions)
        {
            foreach (var action in form.GridView.ToolBarActions)
            {
                if (action is not LegendAction && action is not RefreshAction && action is not FilterAction && action is not ConfigAction && action is not SortAction)
                {
                    action.SetVisible(false);
                }
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

        var script = $"jjview.setLookup('{lookupParameters.ComponentName}','{{{lookupParameters.FieldKey}}}');";
        var selAction = new ScriptAction();
        selAction.Name = "jjselLookup";
        selAction.Icon = IconType.ChevronRight;
        selAction.ToolTip = Translate.Key("Select");
        selAction.OnClientClick = script;
        selAction.IsDefaultOption = true;
        selAction.Order = 100;
        form.GridView.GridActions.Add(selAction);

        foreach (var filter in lookupParameters.Filters)
        {
            form.SetCurrentFilter(filter.Key.ToString(), filter.Value?.ToString());
        }
    }
    
}