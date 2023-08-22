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
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
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

    public async Task<ActionResult> Index(string dictionaryName)
    {
        var formElement = await _actionsService.DataDictionaryRepository.GetMetadataAsync(dictionaryName);
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

        var formElement = await _actionsService.DataDictionaryRepository.GetMetadataAsync(dictionaryName);

        var action = context switch
        {
            ActionSource.GridTable => formElement.Options.GridTableActions.Get(actionName),
            ActionSource.GridToolbar => formElement.Options.GridToolbarActions.Get(actionName),
            ActionSource.FormToolbar => formElement.Options.FormToolbarActions.Get(actionName),
            ActionSource.Field => formElement.Fields[fieldName].Actions.Get(actionName),
            _ => null
        };
        
        var iconSearchBoxResult = await GetIconSearchBoxResult(action);

        if (iconSearchBoxResult.IsActionResult())
            return iconSearchBoxResult.ToActionResult();

        ViewBag.IconSearchBoxHtml = iconSearchBoxResult.Content; 
        
        if (action is InsertAction insertAction)
        {
            var insertSearchBoxResult = await GetInsertSearchBoxResult(insertAction);
            ViewBag.InsertSearchBoxHtml = insertSearchBoxResult.Content!;
        }

        await PopulateViewBag(dictionaryName, action!, context, fieldName);

        return View(action!.GetType().Name, action);
        
    }

    private async Task<ComponentResult> GetIconSearchBoxResult(BasicAction? action)
    {
        var iconSearchBox = _searchBoxFactory.Create();
        iconSearchBox.DataItem.ShowImageLegend = true;
        iconSearchBox.DataItem.Items = Enum.GetValues<IconType>()
            .Select(i => new DataItemValue(i.GetId().ToString(), i.GetDescription(), i, "6a6a6a")).ToList();
        iconSearchBox.SelectedValue = ((int)action!.Icon).ToString();
        iconSearchBox.Name = "icon";

        var iconSearchBoxResult = await iconSearchBox.GetResultAsync();
        return iconSearchBoxResult;
    }

    private async Task<ComponentResult> GetInsertSearchBoxResult(InsertAction insertAction)
    {
        var searchBox = _searchBoxFactory.Create();
        searchBox.Name = "ElementNameToSelect";
        searchBox.DataItem.Command.Sql =
            $"select name as cod, name from {_options.DataDictionaryTableName} where type = 'F' order by name";
        searchBox.SelectedValue = insertAction.ElementNameToSelect;

        var result = await searchBox.GetResultAsync();
        return result;
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
    
    private async Task<IActionResult> EditActionResult<TAction>(
        string dictionaryName, 
        TAction action, 
        ActionSource context, 
        bool isActionSave,
        string? originalName = null, 
        string? fieldName = null
        ) where TAction : BasicAction
    {
        if (isActionSave)
        {
            await SaveAction(dictionaryName, action, context, originalName, fieldName);
        }
    
        var iconSearchBoxResult = await GetIconSearchBoxResult(action);

        if (iconSearchBoxResult.IsActionResult())
            return iconSearchBoxResult.ToActionResult();

        await PopulateViewBag(dictionaryName, action, context);
        return View(action);
    }
    

    [HttpPost]
    public async Task<ActionResult> Remove(string dictionaryName, string actionName, ActionSource context, string? fieldName)
    {
        await _actionsService.DeleteActionAsync(dictionaryName, actionName, context, fieldName);
        return Json(new { success = true });
    }


    [HttpPost]
    public async Task<ActionResult> Sort(string dictionaryName, string[] orderFields, ActionSource context, string? fieldName)
    {
        await _actionsService.SortActionsAsync(dictionaryName, orderFields, context, fieldName);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<ActionResult> EnableDisable(string dictionaryName, string actionName, ActionSource context, bool value)
    {
        await _actionsService.EnableDisable(dictionaryName, actionName, context, value);
        return Json(new { success = true });
    }


    [HttpPost]
    public async Task<IActionResult> InsertAction(string dictionaryName, InsertAction insertAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,insertAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ConfigAction(string dictionaryName, ConfigAction configAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,configAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ExportAction(string dictionaryName, ExportAction exportAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,exportAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ViewAction(string dictionaryName, ViewAction viewAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,viewAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> EditAction(string dictionaryName, EditAction editAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,editAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAction(string dictionaryName, DeleteAction deleteAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,deleteAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ImportAction(string dictionaryName, ImportAction importAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,importAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> RefreshAction(string dictionaryName, RefreshAction refreshAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,refreshAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> LegendAction(string dictionaryName, LegendAction legendAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,legendAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> SortAction(string dictionaryName, SortAction sortAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,sortAction,context,isActionSave,originalName);
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAction(string dictionaryName, SaveAction saveAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,saveAction,context,isActionSave,originalName);
    }
    
    [HttpPost]
    public async Task<IActionResult> CancelAction(string dictionaryName, CancelAction cancelAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,cancelAction,context,isActionSave,originalName);
    }
    
    [HttpPost]
    public async Task<IActionResult> BackAction(string dictionaryName, BackAction cancelAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,cancelAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> LogAction(string dictionaryName, LogAction logAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,logAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> FilterAction(string dictionaryName, FilterAction filterAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,filterAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> UrlRedirectAction(string dictionaryName, UrlRedirectAction urlAction, ActionSource context,
        string? fieldName, string? originalName, bool isActionSave)
    {
        return await EditActionResult(dictionaryName,urlAction,context,isActionSave,originalName,fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> ScriptAction(string dictionaryName, ScriptAction scriptAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return await EditActionResult(dictionaryName,scriptAction,context,isActionSave,originalName,fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> SqlCommandAction(string dictionaryName, SqlCommandAction sqlAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return await EditActionResult(dictionaryName,sqlAction,context,isActionSave,originalName,fieldName);
    }
    

    [HttpPost]
    public async Task<IActionResult> InternalAction(string dictionaryName, InternalAction internalAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return await EditActionResult(dictionaryName,internalAction,context,isActionSave,originalName,fieldName);
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
                ViewBag.ElementNameList = _actionsService.GetElementListAsync();
                ViewBag.InternalFieldList = _actionsService.GetFieldList(dictionaryName);
                string elementNameRedirect = internalAction.ElementRedirect.ElementNameRedirect;
                ViewBag.RedirectFieldList = _actionsService.GetFieldList(elementNameRedirect);
                break;
            }
        }
    }


}