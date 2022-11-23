using System.Collections;
using System.Data.SqlClient;
using System.Web;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Controllers;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
public class InternalRedirectController : MasterDataController
{
    private string? _dictionaryName;
    private RelationType _relationType;
    private Hashtable? _relationValues;

    public ActionResult Index(string parameters)
    {
        LoadParameters(parameters);
        var userId = HttpContext.GetUserId();
        bool isAjax = HttpContext.Request.Query.ContainsKey("uploadValues");
        
        InternalRedirectViewModel model;
        
        switch (_relationType)
        {
            case RelationType.List:
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
            case RelationType.View:
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
            case RelationType.Update:
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
            panel.SetUserValues("USERID", userId.ToString());

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
        _relationType = RelationType.List;
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
                    _relationType = (RelationType)int.Parse(@params.Get(key));
                    break;
                default:
                    _relationValues.Add(key, @params.Get(key));
                    break;
            }
        }
    }
}