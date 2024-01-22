#nullable enable

using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.UI.Components;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ActionsController(ActionsService actionsService,
        IEnumerable<IPluginHandler> pluginHandlers)
    : DataDictionaryController
{
    public async Task<ActionResult> Index(string elementName)
    {
        var formElement = await actionsService.DataDictionaryRepository.GetFormElementAsync(elementName);
        var model = new ActionsListViewModel(elementName, "Actions")
        {
            GridTableActions = formElement.Options.GridTableActions.GetAllSorted(),
            GridToolbarActions = formElement.Options.GridToolbarActions.GetAllSorted(),
            FormToolbarActions = formElement.Options.FormToolbarActions.GetAllSorted()
        };

        if (Request.HasFormContentType && Request.Form.TryGetValue("selected-tab", out var selectedTab))
            ViewBag.Tab = selectedTab;

        return View(model);
    }

    public async Task<IActionResult> Edit(string elementName, string actionName, ActionSource context, string fieldName)
    {
        if (elementName is null)
            throw new ArgumentNullException(nameof(elementName));


        var formElement = await actionsService.DataDictionaryRepository.GetFormElementAsync(elementName);

        var action = context switch
        {
            ActionSource.GridTable => formElement.Options.GridTableActions.Get(actionName),
            ActionSource.GridToolbar => formElement.Options.GridToolbarActions.Get(actionName),
            ActionSource.FormToolbar => formElement.Options.FormToolbarActions.Get(actionName),
            ActionSource.Field => formElement.Fields[fieldName].Actions.Get(actionName),
            _ => null
        };

        await PopulateViewBag(elementName, action!, context, fieldName);

        return View(action!.GetType().Name, action);
    }

    public async Task<IActionResult> Add(
        string elementName,
        string actionType, 
        ActionSource context, 
        string? fieldName = null,
        Guid? pluginId = null
        )
    {
        BasicAction action = actionType switch
        {
            nameof(ScriptAction) => new ScriptAction(),
            nameof(UrlRedirectAction) => new UrlRedirectAction(),
            nameof(InternalAction) => new InternalAction(),
            nameof(SqlCommandAction) => new SqlCommandAction(),
            nameof(PluginAction) => new PluginAction
            {
                PluginId = pluginId!.Value
            },
            nameof(PluginFieldAction) => new PluginFieldAction
            {
                PluginId = pluginId!.Value
            },
            _ => throw new JJMasterDataException("Invalid Action")
        };

        await PopulateViewBag(elementName, action, context, fieldName);
        return View(action.GetType().Name, action);
    }

    private async Task<IActionResult> EditActionResult<TAction>(
        string elementName,
        TAction action,
        ActionSource context,
        bool isActionSave,
        string? originalName = null,
        string? fieldName = null
    ) where TAction : BasicAction
    {
        if (isActionSave)
        {
            await SaveAction(elementName, action, context, originalName, fieldName);
        }
        
        await PopulateViewBag(elementName, action, context, fieldName);
        
        return View(action);
    }


    [HttpPost]
    public async Task<ActionResult> Remove(string elementName, string actionName, ActionSource context,
        string? fieldName)
    {
        await actionsService.DeleteActionAsync(elementName, actionName, context, fieldName);
        return Json(new { success = true });
    }


    [HttpPost]
    public async Task<ActionResult> Sort(string elementName, string fieldsOrder, ActionSource context,
        string? fieldName)
    {
        await actionsService.SortActionsAsync(elementName, fieldsOrder.Split(","), context, fieldName);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<ActionResult> EnableDisable(string elementName, string actionName, ActionSource context,
        bool visibility)
    {
        await actionsService.EnableDisable(elementName, actionName, context, visibility);
        return Json(new { success = true });
    }


    [HttpPost]
    public Task<IActionResult> InsertAction(string elementName, InsertAction insertAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, insertAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ConfigAction(string elementName, ConfigAction configAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, configAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ExportAction(string elementName, ExportAction exportAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, exportAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ViewAction(string elementName, ViewAction viewAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, viewAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> EditAction(string elementName, EditAction editAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, editAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> DeleteAction(string elementName, DeleteAction deleteAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, deleteAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ImportAction(string elementName, ImportAction importAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, importAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> RefreshAction(string elementName, RefreshAction refreshAction,
        ActionSource context, string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, refreshAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> LegendAction(string elementName, LegendAction legendAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, legendAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> SortAction(string elementName, SortAction sortAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, sortAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> SaveAction(string elementName, SaveAction saveAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, saveAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> CancelAction(string elementName, CancelAction cancelAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, cancelAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> BackAction(string elementName, BackAction cancelAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, cancelAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> FormEditAction(string elementName, FormEditAction cancelAction,
        ActionSource context, string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, cancelAction, context, isActionSave, originalName);
    }


    [HttpPost]
    public Task<IActionResult> AuditLogGridToolbarAction(string elementName,
        AuditLogGridToolbarAction auditLogAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, auditLogAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> AuditLogFormToolbarAction(string elementName,
        AuditLogFormToolbarAction auditLogAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, auditLogAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> FilterAction(string elementName, FilterAction filterAction, ActionSource context,
        string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, filterAction, context, isActionSave, originalName);
    }

    [HttpPost]
    public Task<IActionResult> UrlRedirectAction(string elementName, UrlRedirectAction urlAction,
        ActionSource context,
        string? fieldName, string? originalName, bool isActionSave)
    {
        return EditActionResult(elementName, urlAction, context, isActionSave, originalName, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> ScriptAction(string elementName, ScriptAction scriptAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return EditActionResult(elementName, scriptAction, context, isActionSave, originalName, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> SqlCommandAction(string elementName, SqlCommandAction sqlAction,
        ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return EditActionResult(elementName, sqlAction, context, isActionSave, originalName, fieldName);
    }


    [HttpPost]
    public Task<IActionResult> InternalAction(string elementName, InternalAction internalAction,
        ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return EditActionResult(elementName, internalAction, context, isActionSave, originalName, fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> AddRelation(string elementName, InternalAction internalAction,
        ActionSource context,
        string redirectField, string internalField, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.Add(new FormActionRelationField
        {
            RedirectField = redirectField,
            InternalField = internalField
        });

        await PopulateViewBag(elementName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRelation(string elementName, InternalAction internalAction,
        ActionSource context,
        int relationIndex, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.RemoveAt(relationIndex);
        await PopulateViewBag(elementName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }
    
    [HttpPost]
    public Task<IActionResult> PluginAction(
        string elementName, 
        PluginAction pluginAction,
        ActionSource context,
        string? originalName, 
        bool isActionSave,
        string? fieldName)
    {
        SetPluginConfigurationMap(pluginAction.ConfigurationMap, pluginAction.PluginId);
        
        return EditActionResult(elementName, pluginAction, context, isActionSave, originalName, fieldName);
    }
    [HttpPost]
    public Task<IActionResult> PluginFieldAction(
        string elementName, 
        PluginFieldAction pluginFieldAction,
        ActionSource context,
        string? originalName, 
        bool isActionSave,
        string? fieldName)
    {
        SetPluginConfigurationMap(pluginFieldAction.ConfigurationMap, pluginFieldAction.PluginId);
        
        SetPluginFieldMap(pluginFieldAction.FieldMap, pluginFieldAction.PluginId);
        
        return EditActionResult(elementName, pluginFieldAction, context, isActionSave, originalName, fieldName);
    }

    private void SetPluginConfigurationMap(IDictionary<string, object?> configurationMap,
        Guid pluginId)
    {
        var pluginHandler = pluginHandlers.First(p => p.Id == pluginId);

        if (pluginHandler.ConfigurationFields == null)
            return;
        
        foreach (var field in pluginHandler.ConfigurationFields)
        {
            if (Request.Form.TryGetValue(field.Name, out var value))
            {
                var pluginField = pluginHandler.ConfigurationFields.First(f => f.Name == field.Name);

                configurationMap[field.Name] = pluginField.Type switch
                {
                    PluginConfigurationFieldType.Boolean => value == "true",
                    PluginConfigurationFieldType.Number => double.Parse(value.ToString()),
                    _ => value.ToString()
                };
            }
        }
    }
    
    private void SetPluginFieldMap(IDictionary<string, string> fieldMap, Guid pluginId)
    {
        var pluginHandler = (IPluginFieldActionHandler)pluginHandlers.First(p => p.Id == pluginId);
        
        foreach (var key in pluginHandler.FieldMapKeys)
        {
            if (Request.Form.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            {
                fieldMap[key] = value!;
            }
        }
    }

    private async Task SaveAction(string elementName, BasicAction basicAction, ActionSource context,
        string? originalName, string? fieldName = null)
    {
        await actionsService.SaveAction(elementName, basicAction, context, originalName, fieldName);

        if (ModelState.IsValid)
            ViewBag.Success = true;
        else
            ViewBag.Error = actionsService.GetValidationSummary().GetHtml();
    }

    private async Task PopulateViewBag(string elementName, BasicAction basicAction, ActionSource context,
        string? fieldName = null)
    {
        if (Request.HasFormContentType && Request.Form.TryGetValue("originalName", out var originalName))
            ViewBag.OriginalName = originalName;
        else
            ViewBag.OriginalName = basicAction.Name;

        if (Request.HasFormContentType && Request.Form.TryGetValue("selected-tab", out var selectedTab)) 
            ViewBag.Tab = selectedTab;

        else if (TempData.TryGetValue("selected-tab",  out var tempSelectedTab))
            ViewBag.Tab = tempSelectedTab?.ToString()!;
        
        ViewBag.ElementName = elementName;
        ViewBag.ContextAction = context;
        ViewBag.MenuId = "Actions";
        ViewBag.FieldName = fieldName!;
        var formElement = await actionsService.GetFormElementAsync(elementName);
        ViewBag.FormElement = formElement;
        ViewBag.CodeMirrorHintList = JsonConvert.SerializeObject(actionsService.GetAutocompleteHintsList(formElement, includeAdditionalHints:false));

        if (basicAction is InternalAction internalAction)
        {
            ViewBag.ElementNameList = await actionsService.GetElementListAsync();
            ViewBag.InternalFieldList = await actionsService.GetFieldList(elementName);
            var elementNameRedirect = internalAction.ElementRedirect.ElementNameRedirect;
            ViewBag.RedirectFieldList = await actionsService.GetFieldList(elementNameRedirect);
        }
    }
    
}