#nullable enable

using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class ActionsController : DataDictionaryController
{
    private readonly ActionsService _actionsService;

    public ActionsController(ActionsService actionsService)
    {
        _actionsService = actionsService;
    }

    public ActionResult Index(string dictionaryName)
    {
        var dicParcer = _actionsService.DataDictionaryRepository.GetMetadata(dictionaryName);
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.MenuId = "Actions";
        ViewBag.ToolBarActions = dicParcer.UIOptions.ToolBarActions.GetAll();
        ViewBag.GridActions = dicParcer.UIOptions.GridActions.GetAll();

        if ((string?)Request.Query["selected_tab"] == null)
            ViewBag.Tab = Request.Query["selected_tab"];

        return View();
    }

    public ActionResult Edit(string dictionaryName, string actionName, ActionOrigin context, string fieldName)
    {
        if (dictionaryName is null)
        {
            throw new ArgumentNullException(nameof(dictionaryName));
        }

        var metadata = _actionsService.DataDictionaryRepository.GetMetadata(dictionaryName);

        BasicAction? action = null;
        switch (context)
        {
            case ActionOrigin.Grid:
                action = metadata.UIOptions.GridActions.Get(actionName);
                break;
            case ActionOrigin.Toolbar:
                action = metadata.UIOptions.ToolBarActions.Get(actionName);
                break;
            case ActionOrigin.Field:
                var formElement = metadata.GetFormElement();
                action = formElement.Fields[fieldName].Actions.Get(actionName);
                break;
        }

  
        PopulateViewBag(dictionaryName, action!, context, fieldName);
        return View(action!.GetType().Name, action);
        
    }

    public ActionResult Add(string dictionaryName, string actionType, ActionOrigin context, string? fieldName)
    {
        BasicAction action = actionType switch
        {
            nameof(ScriptAction) => new ScriptAction(),
            nameof(UrlRedirectAction) => new UrlRedirectAction(),
            nameof(InternalAction) => new InternalAction(),
            nameof(SqlCommandAction) => new SqlCommandAction(),
            nameof(PythonScriptAction) => new PythonScriptAction(),
            _ => throw new JJMasterDataException("Invalid Action")
        };

        PopulateViewBag(dictionaryName, action, context, fieldName);
        return View(action.GetType().Name, action);
    }


    [HttpPost]
    public ActionResult Remove(string dictionaryName, string actionName, ActionOrigin context, string? fieldName)
    {
        _actionsService.DeleteAction(dictionaryName, actionName, context, fieldName);
        return Json(new { success = true });
    }


    [HttpPost]
    public ActionResult Sort(string dictionaryName, string[] orderFields, ActionOrigin context, string? fieldName)
    {
        _actionsService.SortActions(dictionaryName, orderFields, context, fieldName);
        return Json(new { success = true });
    }

    [HttpPost]
    public ActionResult EnableDisable(string dictionaryName, string actionName, ActionOrigin context, bool value)
    {
        _actionsService.EnableDisable(dictionaryName, actionName, context, value);
        return Json(new { success = true });
    }


    [HttpPost]
    public ActionResult InsertAction(string dictionaryName, InsertAction insertAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, insertAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, insertAction, context);
        return View(insertAction);
    }

    [HttpPost]
    public ActionResult ConfigAction(string dictionaryName, ConfigAction configAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, configAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, configAction, context);
        return View(configAction);
    }

    [HttpPost]
    public ActionResult ExportAction(string dictionaryName, ExportAction exportAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, exportAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, exportAction, context);
        return View(exportAction);
    }

    [HttpPost]
    public ActionResult ViewAction(string dictionaryName, ViewAction viewAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, viewAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, viewAction, context);
        return View(viewAction);
    }

    [HttpPost]
    public ActionResult EditAction(string dictionaryName, EditAction editAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, editAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, editAction, context);
        return View(editAction);
    }

    [HttpPost]
    public ActionResult DeleteAction(string dictionaryName, DeleteAction deleteAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, deleteAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, deleteAction, context);
        return View(deleteAction);
    }

    [HttpPost]
    public ActionResult ImportAction(string dictionaryName, ImportAction importAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, importAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, importAction, context);
        return View(importAction);
    }

    [HttpPost]
    public ActionResult RefreshAction(string dictionaryName, RefreshAction refreshAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, refreshAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, refreshAction, context);
        return View(refreshAction);
    }

    [HttpPost]
    public ActionResult LegendAction(string dictionaryName, LegendAction legendAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, legendAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, legendAction, context);
        return View(legendAction);
    }

    [HttpPost]
    public ActionResult SortAction(string dictionaryName, SortAction sortAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, sortAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, sortAction, context);
        return View(sortAction);
    }

    [HttpPost]
    public ActionResult LogAction(string dictionaryName, LogAction logAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, logAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, logAction, context);
        return View(logAction);
    }

    [HttpPost]
    public ActionResult FilterAction(string dictionaryName, FilterAction filterAction, ActionOrigin context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, filterAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, filterAction, context);
        return View(filterAction);
    }

    [HttpPost]
    public ActionResult UrlRedirectAction(string dictionaryName, UrlRedirectAction urlAction, ActionOrigin context,
        string? fieldName, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, urlAction, context, originalName, fieldName);
        }

        PopulateViewBag(dictionaryName, urlAction, context, fieldName);
        return View(urlAction);
    }

    [HttpPost]
    public ActionResult ScriptAction(string dictionaryName, ScriptAction scriptAction, ActionOrigin context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, scriptAction, context, originalName, fieldName);
        }

        PopulateViewBag(dictionaryName, scriptAction, context, fieldName);
        return View(scriptAction);
    }

    [HttpPost]
    public ActionResult SqlCommandAction(string dictionaryName, SqlCommandAction sqlAction, ActionOrigin context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, sqlAction, context, originalName, fieldName);
        }

        PopulateViewBag(dictionaryName, sqlAction, context, fieldName);
        return View(sqlAction);
    }

    [HttpPost]
    public ActionResult PythonScriptAction(string dictionaryName, PythonScriptAction pythonAction, ActionOrigin context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, pythonAction, context, originalName, fieldName);
        }

        PopulateViewBag(dictionaryName, pythonAction, context, fieldName);
        return View(pythonAction);
    }

    [HttpPost]
    public ActionResult InternalAction(string dictionaryName, InternalAction internalAction, ActionOrigin context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, internalAction, context, originalName, fieldName);
        }

        PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction);
    }

    [HttpPost]
    public ActionResult AddRelation(string dictionaryName, InternalAction internalAction, ActionOrigin context,
        string redirectField, string internalField, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.Add(new FormActionRelationField
        {
            RedirectField = redirectField,
            InternalField = internalField
        });

        PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    [HttpPost]
    public ActionResult RemoveRelation(string dictionaryName, InternalAction internalAction, ActionOrigin context,
        int relationIndex, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.RemoveAt(relationIndex);
        PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    private void SaveAction(string dictionaryName, BasicAction basicAction, ActionOrigin context,string? originalName,string? fieldName = null)
    {
        _actionsService.SaveAction(dictionaryName, basicAction, context, originalName, fieldName);
        
        if (ModelState.IsValid)
            ViewBag.Success = true;
        else
            ViewBag.Error = _actionsService.GetValidationSummary().GetHtml();
    }

    private void PopulateViewBag(string dictionaryName, BasicAction basicAction, ActionOrigin context, string? fieldName = null)
    {
        if ((string?)Request.Query["selected_tab"] != null)
            ViewBag.Tab = Request.Query["selected_tab"];
        else if (TempData["selected_tab"] != null)
            ViewBag.Tab = TempData["selected_tab"]!;

        if ((string?)Request.Query["originalName"] != null)
            ViewBag.OriginalName = Request.Query["originalName"];
        else if (TempData["originalName"] != null)
            ViewBag.OriginalName = TempData["originalName"]!;
        else
            ViewBag.OriginalName = basicAction.Name;

        if (TempData.ContainsKey("error"))
            ViewBag.Error = TempData["error"]!;

        ViewBag.DictionaryName = dictionaryName;
        ViewBag.ContextAction = context;
        ViewBag.MenuId = "Actions";
        ViewBag.FieldName = fieldName!;

        switch (basicAction)
        {
            case PythonScriptAction _:
            case SqlCommandAction _:
            case ImportAction _:
            case ExportAction _:
            {
                var formElement = _actionsService.GetFormElement(dictionaryName);
                ViewBag.HintDictionary = _actionsService.GetHintDictionary(formElement);
                break;
            }
            case InternalAction internalAction:
            {
                ViewBag.ElementNameList = _actionsService.GetElementList();
                ViewBag.InternalFieldList = _actionsService.GetFieldList(dictionaryName);
                string elementNameRedirect = internalAction.ElementRedirect.ElementNameRedirect;
                ViewBag.RedirectFieldList = _actionsService.GetFieldList(elementNameRedirect);
                break;
            }
        }
    }

}