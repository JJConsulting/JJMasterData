using System.Collections;
using System.Web;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class LookupController : MasterDataController
{
    private FormViewFactory FormViewFactory { get; }

    public LookupController(FormViewFactory formViewFactory)
    {
        FormViewFactory = formViewFactory;
    }
    
    // GET: MasterData/Lookup
    public async Task<IActionResult> Index(string p)
    {
        if (string.IsNullOrEmpty(p))
            throw new ArgumentNullException();

        string? elementName = null;
        string? fieldKey = null;
        string? objid = null;
        bool enableAction = false;

        var filters = new Hashtable();
        var parms = HttpUtility.ParseQueryString(Cript.EnigmaDecryptRP(p));
        foreach (string key in parms)
        {
            if ("elementname".Equals(key.ToLower()))
                elementName = parms.Get(key);
            else if ("fieldkey".Equals(key.ToLower()))
                fieldKey = parms.Get(key);
            else if ("objid".Equals(key.ToLower()))
                objid = parms.Get(key);
            else if ("enableaction".Equals(key.ToLower()))
                enableAction = parms.Get(key)!.Equals("1");
            else
                filters.Add(key, parms.Get(key));
        }
            
        if (elementName == null | objid == null)
            throw new JJMasterDataException(Translate.Key("Invalid Parameter"));

        //FormView
        var form = await FormViewFactory.CreateFormViewAsync(elementName);
        form.ShowTitle = false;

        //Actions
        if (!enableAction)
        {
            foreach (var action in form.ToolBarActions)
            {
                if (action is not LegendAction &
                    action is not RefreshAction &
                    action is not FilterAction &
                    action is not ConfigAction &
                    action is not SortAction)
                {
                    action.SetVisible(false);
                }
            }

            foreach (var action in form.GridActions)
            {
                action.SetVisible(false);
            }
        }

        //Custon Action Select
        foreach (BasicAction action in form.GridActions)
        {
            action.IsDefaultOption = false;
        }

        var script = $"jjview.setLookup('{objid}','{{{fieldKey}}}');";
        var selAction = new ScriptAction();
        selAction.Name = "jjselLookup";
        selAction.Icon = IconType.ChevronRight;
        selAction.ToolTip = Translate.Key("Select");
        selAction.OnClientClick = script;
        selAction.IsDefaultOption = true;
        selAction.Order = 100;
        form.GridActions.Add(selAction);

        //Filters
        foreach (DictionaryEntry filter in filters)
        {
            form.SetCurrentFilter(filter.Key.ToString(), filter.Value?.ToString());
        }

        ViewBag.HtmlPage = form.GetHtml();

        return View(); 
    }
}