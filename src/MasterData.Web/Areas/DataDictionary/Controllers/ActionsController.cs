#nullable enable

using System.Globalization;
using JJConsulting.Html.Bootstrap.TagHelpers.Extensions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ActionsController(ActionsService actionsService,
        IEnumerable<IPluginHandler> pluginHandlers)
    : DataDictionaryController
{
    public async Task<ActionResult> Index(string elementName, string? actionName = null, ActionSource source = ActionSource.GridTable, string? fieldName = null)
    {
        var formElement = await actionsService.GetFormElementAsync(elementName);

        var selectedAction = formElement.GetAction(actionName, source, fieldName);
        
        var model = await BuildActionListModel(elementName, source, selectedAction, fieldName);
        
        await PopulateViewData(formElement, selectedAction, source, fieldName);
        
        return View(model);
    }

    public async Task<IActionResult> Edit(string elementName, string actionName, ActionSource source, string fieldName)
    {
        if (elementName is null)
            throw new ArgumentNullException(nameof(elementName));
        
        var formElement = await actionsService.GetFormElementAsync(elementName);

        var action = formElement.GetAction(actionName, source, fieldName);
        
        await PopulateViewData(formElement, action!, source, fieldName);
 
        return PartialView(action!.GetType().Name, action);

    }

    public async Task<IActionResult> Add(
        string elementName,
        string actionType, 
        ActionSource source, 
        string? fieldName = null,
        Guid? pluginId = null
        )
    {
        var action = CreateActionFromType(actionType, pluginId);

        await PopulateViewData(elementName, action, source, fieldName);
     
        return PartialView(actionType, action);
     
    }

    private static BasicAction CreateActionFromType(string actionType, Guid? pluginId)
    {
        return actionType switch
        {
            nameof(ScriptAction) => new ScriptAction(),
            nameof(UrlRedirectAction) => new UrlRedirectAction(),
            nameof(InternalAction) => new InternalAction(),
            nameof(SqlCommandAction) => new SqlCommandAction(),
            nameof(HtmlTemplateAction) => new HtmlTemplateAction(),
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
    }
    

    private async Task<IActionResult> EditActionResult<TAction>(
        string elementName,
        TAction action,
        ActionSource source,
        string? originalName = null,
        string? fieldName = null
    ) where TAction : BasicAction
    {
        fieldName = ResolveFieldName(source, fieldName);

        await actionsService.SaveAction(elementName, action, source, originalName, fieldName);

        if (ModelState.IsValid)
        {
            await RemoveActionFromOriginalField(elementName, action, source, originalName, fieldName);
        }
        
        var model = await BuildActionListModel(elementName, source, action, fieldName);
        await PopulateViewData(elementName, action, source, fieldName);
        
        ViewData["ShowSaveSuccess"] = ModelState.IsValid;
        
        return View("Index", model);
    }

    private async Task<IActionResult> CopyActionResult<TAction>(
        string elementName,
        TAction action,
        ActionSource source,
        string? fieldName = null
    ) where TAction : BasicAction
    {
        fieldName = ResolveFieldName(source, fieldName);
        
        await actionsService.SaveAction(elementName, action, source, null, fieldName);

        var model = await BuildActionListModel(elementName, source, action, fieldName);
        await PopulateViewData(elementName, action, source, fieldName);
        
        ViewData["ShowCopySuccess"] = ModelState.IsValid;
        
        return View("Index", model);
    }
    

    private string? GetOriginalFieldName()
    {
        if (Request.HasFormContentType && Request.Form.TryGetValue("originalFieldName", out var value))
            return value.ToString();

        return null;
    }

    private async Task RemoveActionFromOriginalField<TAction>(string elementName, TAction action, ActionSource source,
        string? originalName, string? fieldName) where TAction : BasicAction
    {
        if (source != ActionSource.Field)
            return;

        var originalFieldName = GetOriginalFieldName();
        if (string.IsNullOrWhiteSpace(originalFieldName) || string.IsNullOrWhiteSpace(fieldName))
            return;

        if (string.Equals(originalFieldName, fieldName, StringComparison.OrdinalIgnoreCase))
            return;

        var actionNameToRemove = !string.IsNullOrWhiteSpace(originalName) ? originalName : action.Name;
        await actionsService.DeleteActionAsync(elementName, actionNameToRemove, ActionSource.Field, originalFieldName);
    }


    [HttpPost]
    public async Task<ActionResult> Remove(string elementName, string actionName, ActionSource source,
        string? fieldName)
    {
        await actionsService.DeleteActionAsync(elementName, actionName, source, fieldName);
        return Json(new { success = true });
    }


    [HttpPost]
    public async Task<ActionResult> Sort(string elementName, string fieldsOrder, ActionSource source,
        string? fieldName)
    {
        await actionsService.SortActionsAsync(elementName, fieldsOrder.Split(","), source, fieldName);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<ActionResult> EnableDisable(string elementName, string actionName, ActionSource source,
        bool visibility)
    {
        await actionsService.EnableDisable(elementName, actionName, source, visibility);
        return Json(new { success = true });
    }


    [HttpPost]
    public Task<IActionResult> InsertAction(string elementName, InsertAction insertAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, insertAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ConfigAction(string elementName, ConfigAction configAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, configAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ExportAction(string elementName, ExportAction exportAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, exportAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ViewAction(string elementName, ViewAction viewAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, viewAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> EditAction(string elementName, EditAction editAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, editAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> DeleteAction(string elementName, DeleteAction deleteAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, deleteAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> ImportAction(string elementName, ImportAction importAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, importAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> RefreshAction(string elementName, RefreshAction refreshAction,
        ActionSource source, string? originalName)
    {
        return EditActionResult(elementName, refreshAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> LegendAction(string elementName, LegendAction legendAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, legendAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> SortAction(string elementName, SortAction sortAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, sortAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> SaveAction(string elementName, SaveAction saveAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, saveAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> CancelAction(string elementName, CancelAction cancelAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, cancelAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> BackAction(string elementName, BackAction cancelAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, cancelAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> CopyInsertAction(string elementName, InsertAction insertAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, insertAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyConfigAction(string elementName, ConfigAction configAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, configAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyExportAction(string elementName, ExportAction exportAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, exportAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyViewAction(string elementName, ViewAction viewAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, viewAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyEditAction(string elementName, EditAction editAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, editAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyDeleteAction(string elementName, DeleteAction deleteAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, deleteAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyImportAction(string elementName, ImportAction importAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, importAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyFilterAction(string elementName, FilterAction filterAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, filterAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyRefreshAction(string elementName, RefreshAction refreshAction,
        ActionSource source, string? fieldName)
    {
        return CopyActionResult(elementName, refreshAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyLegendAction(string elementName, LegendAction legendAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, legendAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopySortAction(string elementName, SortAction sortAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, sortAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopySaveAction(string elementName, SaveAction saveAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, saveAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyCancelAction(string elementName, CancelAction cancelAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, cancelAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyBackAction(string elementName, BackAction backAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, backAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyUrlRedirectAction(string elementName, UrlRedirectAction urlRedirectAction,
        ActionSource source, string? fieldName)
    {
        return CopyActionResult(elementName, urlRedirectAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyInternalAction(string elementName, InternalAction internalAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, internalAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopySqlCommandAction(string elementName, SqlCommandAction sqlCommandAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, sqlCommandAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyScriptAction(string elementName, ScriptAction scriptAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, scriptAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyHtmlTemplateAction(string elementName, HtmlTemplateAction htmlTemplateAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, htmlTemplateAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyPluginAction(string elementName, PluginAction pluginAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, pluginAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyPluginFieldAction(string elementName, PluginFieldAction pluginFieldAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, pluginFieldAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyFormEditAction(string elementName, FormEditAction formEditAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, formEditAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyGridEditAction(string elementName, GridEditAction gridEditAction, ActionSource source,
        string? fieldName)
    {
        return CopyActionResult(elementName, gridEditAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyAuditLogFormToolbarAction(string elementName, AuditLogFormToolbarAction auditLogFormToolbarAction,
        ActionSource source, string? fieldName)
    {
        return CopyActionResult(elementName, auditLogFormToolbarAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> CopyAuditLogGridToolbarAction(string elementName, AuditLogGridToolbarAction auditLogGridToolbarAction,
        ActionSource source, string? fieldName)
    {
        return CopyActionResult(elementName, auditLogGridToolbarAction, source, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> FormEditAction(string elementName, FormEditAction cancelAction,
        ActionSource source, string? originalName)
    {
        return EditActionResult(elementName, cancelAction, source, originalName);
    }


    [HttpPost]
    public Task<IActionResult> AuditLogGridToolbarAction(string elementName,
        AuditLogGridToolbarAction auditLogAction, ActionSource source, string? originalName)
    {
        return EditActionResult(elementName, auditLogAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> AuditLogFormToolbarAction(string elementName,
        AuditLogFormToolbarAction auditLogAction, ActionSource source, string? originalName)
    {
        return EditActionResult(elementName, auditLogAction, source, originalName);
    }

    [HttpPost]
    public Task<IActionResult> FilterAction(string elementName, FilterAction filterAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, filterAction, source, originalName);
    }
    
    [HttpPost]
    public Task<IActionResult> GridEditAction(string elementName, GridEditAction gridEditAction, ActionSource source,
        string? originalName)
    {
        return EditActionResult(elementName, gridEditAction, source, originalName);
    }
    
    [HttpPost]
    public Task<IActionResult> UrlRedirectAction(string elementName, UrlRedirectAction urlAction,
        ActionSource source,
        string? fieldName, string? originalName)
    {
        return EditActionResult(elementName, urlAction, source, originalName, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> ScriptAction(string elementName, ScriptAction scriptAction, ActionSource source,
        string? originalName, string? fieldName)
    {
        return EditActionResult(elementName, scriptAction, source, originalName, fieldName);
    }

    [HttpPost]
    public Task<IActionResult> SqlCommandAction(
        string elementName, 
        SqlCommandAction sqlAction,
        ActionSource source,
        string? originalName, string? fieldName)
    {
        return EditActionResult(elementName, sqlAction, source, originalName, fieldName);
    }
    
    [HttpPost]
    public Task<IActionResult> HtmlTemplateAction(
        string elementName,
        HtmlTemplateAction htmlTemplateAction,
        ActionSource source,
        string? originalName, string? fieldName)
    {
        return EditActionResult(elementName, htmlTemplateAction, source, originalName, fieldName);
    }


    [HttpPost]
    public Task<IActionResult> InternalAction(string elementName, InternalAction internalAction,
        ActionSource source,
        string? originalName, string? fieldName)
    {
        return EditActionResult(elementName, internalAction, source, originalName, fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> AddRelation(string elementName, InternalAction internalAction,
        ActionSource source,
        string redirectField, string internalField, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.Add(new FormActionRelationField
        {
            RedirectField = redirectField,
            InternalField = internalField
        });

        await PopulateViewData(elementName, internalAction, source, fieldName);
   
        return PartialView(internalAction.GetType().Name, internalAction);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRelation(string elementName, InternalAction internalAction,
        ActionSource source,
        int relationIndex, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.RemoveAt(relationIndex);
        await PopulateViewData(elementName, internalAction, source, fieldName);
 
        return PartialView(internalAction.GetType().Name, internalAction);
    }
    
    [HttpPost]
    public Task<IActionResult> PluginAction(
        string elementName, 
        PluginAction pluginAction,
        ActionSource source,
        string? originalName, 
        string? fieldName)
    {
        SetPluginConfigurationMap(pluginAction.ConfigurationMap, pluginAction.PluginId);
        
        return EditActionResult(elementName, pluginAction, source, originalName, fieldName);
    }
    [HttpPost]
    public Task<IActionResult> PluginFieldAction(
        string elementName, 
        PluginFieldAction pluginFieldAction,
        ActionSource source,
        string? originalName, 
        string? fieldName)
    {
        SetPluginConfigurationMap(pluginFieldAction.ConfigurationMap, pluginFieldAction.PluginId);
        
        SetPluginFieldMap(pluginFieldAction.FieldMap, pluginFieldAction.PluginId);
        
        return EditActionResult(elementName, pluginFieldAction, source, originalName, fieldName);
    }

    private void SetPluginConfigurationMap(Dictionary<string, object?> configurationMap,
        Guid pluginId)
    {
        var pluginHandler = pluginHandlers.First(p => p.Id == pluginId);

        if (pluginHandler.ConfigurationFields == null)
            return;
        
        foreach (var field in pluginHandler.ConfigurationFields)
        {
            if (Request.Form.TryGetValue(field.Name, out var value))
            {
                configurationMap[field.Name] = field.Type switch
                {
                    PluginConfigurationFieldType.Boolean => value == "true",
                    PluginConfigurationFieldType.Number => double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var numberValue)
                        ? numberValue
                        : null,
                    _ => value.ToString()
                };
            }
        }
    }
    
    private void SetPluginFieldMap(Dictionary<string, string> fieldMap, Guid pluginId)
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

    private async Task<ActionListModel> BuildActionListModel(
        string elementName,
        ActionSource source = ActionSource.GridTable,
        BasicAction? selectedAction = null,
        string? fieldName = null)
    {
        var formElement = await actionsService.GetFormElementAsync(elementName);

        List<BasicFieldAction>? fieldActions = null;
        if (source is ActionSource.Field)
        {
            fieldActions = formElement.Fields
                .Where(f => f.Component.SupportActions)
                .SelectMany(f => f.Actions.GetAllSorted().Select(a => new BasicFieldAction
                {
                    FieldName = f.Name,
                    Action = a
                }))
                .ToList();

            ViewData["FieldActions"] = fieldActions;
        }

        var selectedSourceActions = source switch
        {
            ActionSource.GridTable => formElement.Options.GridTableActions.GetAllSorted().Where(a => !a.IsSystemDefined).ToList(),
            ActionSource.GridToolbar => formElement.Options.GridToolbarActions.GetAllSorted().Where(a => !a.IsSystemDefined).ToList(),
            ActionSource.FormToolbar => formElement.Options.FormToolbarActions.GetAllSorted().Where(a => !a.IsSystemDefined).ToList(),
            ActionSource.Field => fieldActions!.ConvertAll(f => f.Action),
            _ => []
        };
        
        selectedAction ??= selectedSourceActions.FirstOrDefault();

        return new ActionListModel
        {
            ElementName = elementName,
            Source = source,
            Actions = selectedSourceActions,
            SelectedAction = selectedAction,
            SelectedFieldName = fieldName
        };
    }

    private async Task PopulateViewData(string elementName, BasicAction? basicAction, ActionSource source,
        string? fieldName = null)
    {
        var formElement = await actionsService.GetFormElementAsync(elementName);

        await PopulateViewData(formElement, basicAction, source, fieldName);
    }
    
    private async Task PopulateViewData(FormElement formElement, BasicAction? basicAction, ActionSource source,
        string? fieldName = null)
    {
        if (Request.HasFormContentType && Request.Form.TryGetValue("originalName", out var originalName))
            ViewData["OriginalName"] = originalName;
        else
            ViewData["OriginalName"] = basicAction?.Name;
        
        if (TryGetSelectedTabValue(out var selectedTab))
            ViewData["Tab"] = selectedTab;
        
        ViewData["ElementName"] = formElement.Name;
        ViewData["ActionSource"] = source;
        ViewData["MenuId"] = "Actions";
        ViewData["FieldName"] = fieldName;

        ViewData["FormElement"] = formElement;
        ViewData["CodeEditorHints"] = formElement.Fields.Select(f => new CodeEditorHint
        {
            Language = "sql",
            InsertText = f.Name,
            Label = f.Name,
            Details = "Form Element Field",
        }).ToList();

        var actionFields = formElement.Fields
            .Where(f => f.Component.SupportActions)
            .Select(f => new SelectListItem(f.Name, f.Name, f.Name == fieldName))
            .ToList();
        ViewData["ActionFieldList"] = actionFields;

        if (basicAction is InternalAction internalAction)
        {
            ViewData["ElementNameList"] = (await actionsService.GetElementsDictionaryAsync()).OrderBy(n=>n.Key).ToList();
            ViewData["InternalFieldList"] = actionsService.GetFieldList(formElement);
            var elementNameRedirect = internalAction.ElementRedirect.ElementNameRedirect;
            ViewData["RedirectFieldList"] = await actionsService.GetFieldList(elementNameRedirect);
        }
        
        ViewData["InitialSource"] = source;
        ViewData["InitialAction"] = basicAction;
        ViewData["InitialFieldName"] = fieldName;
    }

    private bool TryGetSelectedTabValue(out string selectedTab)
    {
        selectedTab = string.Empty;
        if (!Request.HasFormContentType)
            return false;

        if (Request.Form.TryGetValue("selectedTab", out var selectedTabValue) &&
            !string.IsNullOrWhiteSpace(selectedTabValue))
        {
            selectedTab = selectedTabValue.ToString();
            return true;
        }

        if (Request.Form.TryGetValue("selected-tab", out var legacySelectedTabValue) &&
            !string.IsNullOrWhiteSpace(legacySelectedTabValue))
        {
            selectedTab = legacySelectedTabValue.ToString();
            return true;
        }

        return false;
    }


    private string? ResolveFieldName(ActionSource source, string? fieldName)
    {
        if (source != ActionSource.Field || !Request.HasFormContentType)
            return fieldName;

        if (Request.Form.TryGetValue("fieldName", out var fieldValue) &&
            !string.IsNullOrWhiteSpace(fieldValue))
        {
            return fieldValue.ToString();
        }

        if (Request.Form.TryGetValue("originalFieldName", out var originalFieldValue) &&
            !string.IsNullOrWhiteSpace(originalFieldValue))
        {
            return originalFieldValue.ToString();
        }

        return fieldName;
    }
}
