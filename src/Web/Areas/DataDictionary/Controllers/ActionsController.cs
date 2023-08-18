#nullable enable

using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Options;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ActionsController : DataDictionaryController
{
    private readonly ActionsService _actionsService;
    private readonly IControlFactory<JJSearchBox> _searchBoxFactory;
    private readonly JJMasterDataCoreOptions _options;

    public ActionsController(ActionsService actionsService, IControlFactory<JJSearchBox> searchBoxFactory, IOptions<JJMasterDataCoreOptions> options)
    {
        _actionsService = actionsService;
        _searchBoxFactory = searchBoxFactory;
        _options = options.Value;
    }

    public ActionResult Index(string dictionaryName)
    {
        var formElement = _actionsService.DataDictionaryRepository.GetMetadata(dictionaryName);
        var model = new ActionsListViewModel(dictionaryName, "Actions")
        {
            GridTableActions = formElement.Options.GridTableActions.GetAllSorted(),
            GridToolbarActions = formElement.Options.GridToolbarActions.GetAllSorted(),
            FormToolbarActions = formElement.Options.FormToolbarActions.GetAllSorted()
        };

        if ((string?)Request.Query["selected_tab"] == null)
            ViewBag.Tab = Request.Query["selected_tab"];

        return View(model);
    }

    public async Task<IActionResult> Edit(string dictionaryName, string actionName, ActionSource context, string fieldName)
    {
        if (dictionaryName is null)
        {
            throw new ArgumentNullException(nameof(dictionaryName));
        }

        var metadata = await _actionsService.DataDictionaryRepository.GetMetadataAsync(dictionaryName);

        BasicAction? action = context switch
        {
            ActionSource.GridTable => metadata.Options.GridTableActions.Get(actionName),
            ActionSource.GridToolbar => metadata.Options.GridToolbarActions.Get(actionName),
            ActionSource.FormToolbar => metadata.Options.FormToolbarActions.Get(actionName),
            ActionSource.Field => metadata.Fields[fieldName].Actions.Get(actionName),
            _ => null
        };

        await PopulateViewBag(dictionaryName, action!, context, fieldName);

        return View(action!.GetType().Name, action);
        
    }

    public async Task<IActionResult> Add(string dictionaryName, string actionType, ActionSource context, string? fieldName)
    {
        BasicAction action = actionType switch
        {
            nameof(ScriptAction) => new ScriptAction(),
            nameof(UrlRedirectAction) => new UrlRedirectAction(),
            nameof(InternalAction) => new InternalAction(),
            nameof(SqlCommandAction) => new SqlCommandAction(),
            _ => throw new JJMasterDataException("Invalid Action")
        };

        await PopulateViewBag(dictionaryName, action, context, fieldName);
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
    public async Task<IActionResult> InsertAction(string dictionaryName, InsertAction insertAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, insertAction, context, originalName);
        }
        
        var searchBox = _searchBoxFactory.Create();
        searchBox.Name = "ElementNameToSelect";
        searchBox.DataItem.Command.Sql = $"select name as cod, name from {_options.DataDictionaryTableName} where type = 'F' order by name";
        searchBox.SelectedValue = insertAction.ElementNameToSelect;

        var result = await searchBox.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();
        else
            ViewBag.SearchBoxHtml = result.Content!;
        
        await PopulateViewBag(dictionaryName, insertAction, context);
        return View(insertAction);
    }

    [HttpPost]
    public async Task<IActionResult> ConfigAction(string dictionaryName, ConfigAction configAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, configAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, configAction, context);
        return View(configAction);
    }

    [HttpPost]
    public async Task<IActionResult> ExportAction(string dictionaryName, ExportAction exportAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, exportAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, exportAction, context);
        return View(exportAction);
    }

    [HttpPost]
    public async Task<IActionResult> ViewAction(string dictionaryName, ViewAction viewAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, viewAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, viewAction, context);
        return View(viewAction);
    }

    [HttpPost]
    public async Task<IActionResult> EditAction(string dictionaryName, EditAction editAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, editAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, editAction, context);
        return View(editAction);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAction(string dictionaryName, DeleteAction deleteAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, deleteAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, deleteAction, context);
        return View(deleteAction);
    }

    [HttpPost]
    public async Task<IActionResult> ImportAction(string dictionaryName, ImportAction importAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, importAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, importAction, context);
        return View(importAction);
    }

    [HttpPost]
    public async Task<IActionResult> RefreshAction(string dictionaryName, RefreshAction refreshAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, refreshAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, refreshAction, context);
        return View(refreshAction);
    }

    [HttpPost]
    public async Task<IActionResult> LegendAction(string dictionaryName, LegendAction legendAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, legendAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, legendAction, context);
        return View(legendAction);
    }

    [HttpPost]
    public async Task<IActionResult> SortAction(string dictionaryName, SortAction sortAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, sortAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, sortAction, context);
        return View(sortAction);
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAction(string dictionaryName, SaveAction saveAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, saveAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, saveAction, context);
        return View(saveAction);
    }
    
    [HttpPost]
    public async Task<IActionResult> CancelAction(string dictionaryName, CancelAction cancelAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, cancelAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, cancelAction, context);
        return View(cancelAction);
    }
    
    [HttpPost]
    public async Task<IActionResult> BackAction(string dictionaryName, BackAction cancelAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, cancelAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, cancelAction, context);
        return View(cancelAction);
    }

    [HttpPost]
    public async Task<IActionResult> LogAction(string dictionaryName, LogAction logAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, logAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, logAction, context);
        return View(logAction);
    }

    [HttpPost]
    public async Task<IActionResult> FilterAction(string dictionaryName, FilterAction filterAction, ActionSource context, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, filterAction, context, originalName);
        }

        await PopulateViewBag(dictionaryName, filterAction, context);
        return View(filterAction);
    }

    [HttpPost]
    public async Task<IActionResult> UrlRedirectAction(string dictionaryName, UrlRedirectAction urlAction, ActionSource context,
        string? fieldName, string? originalName, bool isActionSave)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, urlAction, context, originalName, fieldName);
        }

        await PopulateViewBag(dictionaryName, urlAction, context, fieldName);
        return View(urlAction);
    }

    [HttpPost]
    public async Task<IActionResult> ScriptAction(string dictionaryName, ScriptAction scriptAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, scriptAction, context, originalName, fieldName);
        }

        await PopulateViewBag(dictionaryName, scriptAction, context, fieldName);
        return View(scriptAction);
    }

    [HttpPost]
    public async Task<IActionResult> SqlCommandAction(string dictionaryName, SqlCommandAction sqlAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, sqlAction, context, originalName, fieldName);
        }

        await PopulateViewBag(dictionaryName, sqlAction, context, fieldName);
        return View(sqlAction);
    }
    

    [HttpPost]
    public async Task<IActionResult> InternalAction(string dictionaryName, InternalAction internalAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, internalAction, context, originalName, fieldName);
        }

        await PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction);
    }

    [HttpPost]
    public async Task<IActionResult> AddRelation(string dictionaryName, InternalAction internalAction, ActionSource context,
        string redirectField, string internalField, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.Add(new FormActionRelationField
        {
            RedirectField = redirectField,
            InternalField = internalField
        });

        await PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRelation(string dictionaryName, InternalAction internalAction, ActionSource context,
        int relationIndex, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.RemoveAt(relationIndex);
        await PopulateViewBag(dictionaryName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    private async Task SaveAction(string dictionaryName, BasicAction basicAction, ActionSource context,string? originalName,string? fieldName = null)
    {
        await _actionsService.SaveAction(dictionaryName, basicAction, context, originalName, fieldName);
        
        if (ModelState.IsValid)
            ViewBag.Success = true;
        else
            ViewBag.Error = _actionsService.GetValidationSummary().GetHtml();
    }

    private async Task PopulateViewBag(string dictionaryName, BasicAction basicAction, ActionSource context, string? fieldName = null)
    {
        if ((string?)Request.Query["originalName"] != null)
            ViewBag.OriginalName = Request.Query["originalName"];
        else if (TempData["originalName"] != null)
            ViewBag.OriginalName = TempData["originalName"]!;
        else
            ViewBag.OriginalName = basicAction.Name;

        if (TempData.TryGetValue("error", out var value))
            ViewBag.Error = value!;

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
                var formElement = await _actionsService.GetFormElementAsync(dictionaryName);
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