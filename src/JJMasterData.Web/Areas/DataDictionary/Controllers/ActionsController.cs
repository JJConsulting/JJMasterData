#nullable enable

using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ActionsController : DataDictionaryController
{
    private readonly ActionsService _actionsService;

    public ActionsController(ActionsService actionsService)
    {
        _actionsService = actionsService;
    }

    public ActionResult Index(string dictionaryName)
    {
        var formElement = _actionsService.DataDictionaryRepository.GetMetadata(dictionaryName);
        var model = new ActionsListViewModel(dictionaryName, "Actions")
        {
            GridTableActions = formElement.Options.GridTableActions.GetAll(),
            GridToolbarActions = formElement.Options.GridToolbarActions.GetAll(),
            FormToolbarActions = formElement.Options.FormToolbarActions.GetAll()
        };

        if ((string?)Request.Query["selected_tab"] == null)
            ViewBag.Tab = Request.Query["selected_tab"];

        return View(model);
    }

    public ActionResult Edit(string dictionaryName, string actionName, ActionSource context, string fieldName)
    {
        if (dictionaryName is null)
        {
            throw new ArgumentNullException(nameof(dictionaryName));
        }

        var metadata = _actionsService.DataDictionaryRepository.GetMetadata(dictionaryName);

        BasicAction? action = context switch
        {
            ActionSource.GridTable => metadata.Options.GridTableActions.Get(actionName),
            ActionSource.GridToolbar => metadata.Options.GridToolbarActions.Get(actionName),
            ActionSource.FormToolbar => metadata.Options.FormToolbarActions.Get(actionName),
            ActionSource.Field => metadata.Fields[fieldName].Actions.Get(actionName),
            _ => null
        };

        PopulateViewBag(dictionaryName, action!, context, fieldName);

        return View(action!.GetType().Name, action);
        
    }

    public ActionResult Add(string dictionaryName, string actionType, ActionSource context, string? fieldName)
    {
        BasicAction action = actionType switch
        {
            nameof(ScriptAction) => new ScriptAction(),
            nameof(UrlRedirectAction) => new UrlRedirectAction(),
            nameof(InternalAction) => new InternalAction(),
            nameof(SqlCommandAction) => new SqlCommandAction(),
            _ => throw new JJMasterDataException("Invalid Action")
        };

        PopulateViewBag(dictionaryName, action, context, fieldName);
        return View(action.GetType().Name, action);
    }


    [HttpPost]
    public ActionResult Remove(string dictionaryName, string actionName, ActionSource context, string? fieldName)
    {
        _actionsService.DeleteAction(dictionaryName, actionName, context, fieldName);
        return Json(new { success = true });
    }


    [HttpPost]
    public ActionResult Sort(string dictionaryName, string[] orderFields, ActionSource context, string? fieldName)
    {
        _actionsService.SortActions(dictionaryName, orderFields, context, fieldName);
        return Json(new { success = true });
    }

    [HttpPost]
    public ActionResult EnableDisable(string dictionaryName, string actionName, ActionSource context, bool value)
    {
        _actionsService.EnableDisable(dictionaryName, actionName, context, value);
        return Json(new { success = true });
    }


    [HttpPost]
    public ActionResult InsertAction(string dictionaryName, InsertAction insertAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, insertAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, insertAction, context);
        return View(insertAction);
    }

    [HttpPost]
    public ActionResult ConfigAction(string dictionaryName, ConfigAction configAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, configAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, configAction, context);
        return View(configAction);
    }

    [HttpPost]
    public ActionResult ExportAction(string dictionaryName, ExportAction exportAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, exportAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, exportAction, context);
        return View(exportAction);
    }

    [HttpPost]
    public ActionResult ViewAction(string dictionaryName, ViewAction viewAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, viewAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, viewAction, context);
        return View(viewAction);
    }

    [HttpPost]
    public ActionResult EditAction(string dictionaryName, EditAction editAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, editAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, editAction, context);
        return View(editAction);
    }

    [HttpPost]
    public ActionResult DeleteAction(string dictionaryName, DeleteAction deleteAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, deleteAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, deleteAction, context);
        return View(deleteAction);
    }

    [HttpPost]
    public ActionResult ImportAction(string dictionaryName, ImportAction importAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, importAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, importAction, context);
        return View(importAction);
    }

    [HttpPost]
    public ActionResult RefreshAction(string dictionaryName, RefreshAction refreshAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, refreshAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, refreshAction, context);
        return View(refreshAction);
    }

    [HttpPost]
    public ActionResult LegendAction(string dictionaryName, LegendAction legendAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, legendAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, legendAction, context);
        return View(legendAction);
    }

    [HttpPost]
    public ActionResult SortAction(string dictionaryName, SortAction sortAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, sortAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, sortAction, context);
        return View(sortAction);
    }
    
    public IActionResult FormToolbarAction(string dictionaryName, FormToolbarAction formToolbarAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, formToolbarAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, formToolbarAction, context);
        return View("formToolbarAction",formToolbarAction);
    }

    [HttpPost]
    public ActionResult LogAction(string dictionaryName, LogAction logAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, logAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, logAction, context);
        return View(logAction);
    }

    [HttpPost]
    public ActionResult FilterAction(string dictionaryName, FilterAction filterAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            SaveAction(dictionaryName, filterAction, context, originalName);
        }

        PopulateViewBag(dictionaryName, filterAction, context);
        return View(filterAction);
    }

    [HttpPost]
    public ActionResult UrlRedirectAction(string dictionaryName, UrlRedirectAction urlAction, ActionSource context,
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
    public ActionResult ScriptAction(string dictionaryName, ScriptAction scriptAction, ActionSource context,
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
    public ActionResult SqlCommandAction(string dictionaryName, SqlCommandAction sqlAction, ActionSource context,
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
    public ActionResult InternalAction(string dictionaryName, InternalAction internalAction, ActionSource context,
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
    public ActionResult AddRelation(string dictionaryName, InternalAction internalAction, ActionSource context,
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
    public ActionResult RemoveRelation(string dictionaryName, InternalAction internalAction, ActionSource context,
        int relationIndex, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.RemoveAt(relationIndex);
        PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    private void SaveAction(string dictionaryName, BasicAction basicAction, ActionSource context,string? originalName,string? fieldName = null)
    {
        _actionsService.SaveAction(dictionaryName, basicAction, context, originalName, fieldName);
        
        if (ModelState.IsValid)
            ViewBag.Success = true;
        else
            ViewBag.Error = _actionsService.GetValidationSummary().GetHtml();
    }

    private void PopulateViewBag(string dictionaryName, BasicAction basicAction, ActionSource context, string? fieldName = null)
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