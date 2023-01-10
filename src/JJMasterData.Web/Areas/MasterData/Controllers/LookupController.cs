using System.Collections;
using System.Web;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Cryptography.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.WebComponents;
using JJMasterData.Core.WebComponents.Factories;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class LookupController : MasterDataController
{
    public FormViewFactory FormViewFactory { get; }
    public JJMasterDataEncryptionService EncryptionService { get; }

    public LookupController(FormViewFactory formViewFactory, JJMasterDataEncryptionService encryptionService)
    {
        FormViewFactory = formViewFactory;
        EncryptionService = encryptionService;
    }
    // GET: MasterData/Lookup
    public ActionResult Index(string parameters)
    {
        if (string.IsNullOrEmpty(parameters))
            throw new ArgumentNullException(nameof(parameters));

        string? elementName = null;
        string? fieldKey = null;
        string? objid = null;
        bool enableAction = false;

        var filters = new Hashtable();
        var @params = HttpUtility.ParseQueryString(EncryptionService.DecryptString(parameters));
        foreach (string key in @params)
        {
            if ("elementname".Equals(key.ToLower()))
                elementName = @params.Get(key);
            else if ("fieldkey".Equals(key.ToLower()))
                fieldKey = @params.Get(key);
            else if ("objid".Equals(key.ToLower()))
                objid = @params.Get(key);
            else if ("enableaction".Equals(key.ToLower()))
                enableAction = @params.Get(key)!.Equals("1");
            else
                filters.Add(key, @params.Get(key));
        }
            
        if (elementName == null | objid == null)
            throw new JJMasterDataException(Translate.Key("Invalid Parameter"));
        
        var form = FormViewFactory.CreateFormView(elementName);

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