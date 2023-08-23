using System.Collections;
using System.Web;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

public class InternalRedirectController : MasterDataController
{
    private string? _dictionaryName;
    private RelationshipViewType _relationshipType;

    private ComponentFactory ComponentFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private IDictionary<string, object> RelationValues { get; }

    public InternalRedirectController(ComponentFactory componentFactory, IEncryptionService encryptionService)
    {
        ComponentFactory = componentFactory;
        EncryptionService = encryptionService;
        RelationValues = new Dictionary<string, object>();
    }

    public async Task<IActionResult> Index(string parameters)
    {
        LoadParameters(parameters);
        var userId = HttpContext.GetUserId();
        bool isAjax = HttpContext.Request.Query.ContainsKey("uploadViewParams");

        InternalRedirectViewModel model;

        switch (_relationshipType)
        {
            case RelationshipViewType.List:
            {
                var form = await ComponentFactory.FormView.CreateAsync(_dictionaryName);

                var result = await form.GetResultAsync();

                if (result.IsActionResult())
                    return result.ToActionResult();

                form.RelationValues = RelationValues;

                if (userId != null)
                {
                    form.SetUserValues("USERID", userId);
                    await form.GridView.SetCurrentFilterAsync("USERID", userId);
                }

                model = new(result.Content!, false);
                break;
            }
            case RelationshipViewType.View:
            {
                var panel = await ComponentFactory.DataPanel.CreateAsync(_dictionaryName);
                panel.PageState = PageState.View;
                if (userId != null)
                    panel.SetUserValues("USERID", userId);

                var result = await panel.GetResultAsync();

                if (result.IsActionResult())
                    return result.ToActionResult();


                await panel.LoadValuesFromPkAsync(RelationValues);

                model = new(result.Content!, false);
                break;
            }
            case RelationshipViewType.Update:
            {
                var panel = await ComponentFactory.DataPanel.CreateAsync(_dictionaryName);
                panel.PageState = PageState.Update;

                var result = await panel.GetResultAsync();

                if (userId != null)
                    panel.SetUserValues("USERID", userId);

                await panel.LoadValuesFromPkAsync(RelationValues);

                model = new(result.Content!, !isAjax);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Save(string parameters)
    {
        LoadParameters(parameters);

        var userId = HttpContext.GetUserId();

        var panel = await ComponentFactory.DataPanel.CreateAsync(_dictionaryName);
        panel.PageState = PageState.Update;

        await panel.LoadValuesFromPkAsync(RelationValues);
        if (userId != null)
            panel.SetUserValues("USERID", userId);

        var values = await panel.GetFormValuesAsync();
        var errors = await panel.ValidateFieldsAsync(values, PageState.Update);
        var formElement = panel.FormElement;
        try
        {
            if (errors.Count == 0)
            {
                await panel.EntityRepository.SetValuesAsync(formElement, values);
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
        var @params = HttpUtility.ParseQueryString(EncryptionService.DecryptStringWithUrlUnescape(parameters));
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
                    RelationValues.Add(key, @params.Get(key)!);
                    break;
            }
        }
    }
}