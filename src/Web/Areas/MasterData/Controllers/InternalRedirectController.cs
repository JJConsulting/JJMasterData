using System.Collections;
using System.Web;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class InternalRedirectController : MasterDataController
{
    private string? _dictionaryName;
    private RelationshipViewType _relationshipType;
    private Hashtable? _relationValues;

    public ActionResult Index(string parameters)
    {
        LoadParameters(parameters);
        var userId = HttpContext.GetUserId();
        bool isAjax = HttpContext.Request.Query.ContainsKey("uploadValues");
        
        InternalRedirectViewModel model;
        
        switch (_relationshipType)
        {
            case RelationshipViewType.List:
                {
                    var form = new JJFormView(_dictionaryName)
                    {
                        RelationValues = _relationValues
                    };

                    if (userId != null)
                    {
                        form.SetUserValues("USERID", userId);
                        form.SetCurrentFilter("USERID", userId);
                    }

                    model = new(form.GetHtml(), false);
                    break;
                }
            case RelationshipViewType.View:
                {
                    var panel = new JJDataPanel(_dictionaryName)
                    {
                        PageState = PageState.View
                    };
                    if (userId != null)
                        panel.SetUserValues("USERID", userId);

                    panel.LoadValuesFromPK(_relationValues);

                    model = new(panel.GetHtml(), false);
                    break;
                }
            case RelationshipViewType.Update:
                {
                    var panel = new JJDataPanel(_dictionaryName)
                    {
                        PageState = PageState.Update
                    };
                    if (userId != null)
                        panel.SetUserValues("USERID", userId);

                    panel.LoadValuesFromPK(_relationValues);

                    model = new(panel.GetHtml(), !isAjax);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }

        return View(model);
    }

    [HttpPost]
    public ActionResult Save(string parameters)
    {
        LoadParameters(parameters);

        var userId = HttpContext.GetUserId();

        var panel = new JJDataPanel(_dictionaryName)
        {
            PageState = PageState.Update
        };

        panel.LoadValuesFromPK(_relationValues);
        if (userId != null)
            panel.SetUserValues("USERID", userId);

        var values = panel.GetFormValues();
        var errors = panel.ValidateFields(values, PageState.Update);
        var formElement = panel.FormElement;
        try
        {
            if (errors.Count == 0)
            {
                panel.EntityRepository.SetValues(formElement, values);
            }
        }
        catch (SqlException ex)
        {
            errors.Add("DB", ExceptionManager.GetMessage(ex));
        }

        if (errors.Count > 0)
        {
            ViewBag.Error = new JJValidationSummary(errors).GetHtml();
            ViewBag.Success = false;
        }
        else
        {
            ViewBag.Success = true;
        }

        return View("Index");
    }

    private void LoadParameters(string parameters)
    {
        if (string.IsNullOrEmpty(parameters))
            throw new ArgumentNullException();

        _dictionaryName = null;
        _relationshipType = RelationshipViewType.List;
        _relationValues = new Hashtable();
        var @params = HttpUtility.ParseQueryString(Cript.EnigmaDecryptRP(parameters));
        _dictionaryName = @params.Get("formname");
        foreach (string key in @params)
        {
            switch (key.ToLower())
            {
                case "formname":
                    _dictionaryName = @params.Get(key);
                    break;
                case "viewtype":
                    _relationshipType = (RelationshipViewType)int.Parse(@params.Get(key) ?? string.Empty);
                    break;
                default:
                    _relationValues.Add(key, @params.Get(key));
                    break;
            }
        }
    }
}