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
using JJMasterData.Core.Web.Html;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ActionsController : DataDictionaryController
{
    private readonly ActionsService _actionsService;
    private readonly IControlFactory<JJSearchBox> _searchBoxFactory;
    private readonly IControlFactory<JJComboBox> _comboBoxFactory;
    private readonly JJMasterDataCoreOptions _options;

    public ActionsController(ActionsService actionsService, IControlFactory<JJSearchBox> searchBoxFactory, IOptions<JJMasterDataCoreOptions> options, IControlFactory<JJComboBox> comboBoxFactory)
    {
        _actionsService = actionsService;
        _searchBoxFactory = searchBoxFactory;
        _comboBoxFactory = comboBoxFactory;
        _options = options.Value;
    }

    public async Task<ActionResult> Index(string elementName)
    {
        var formElement = await _actionsService.DataDictionaryRepository.GetMetadataAsync(elementName);
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
        

        var formElement = await _actionsService.DataDictionaryRepository.GetMetadataAsync(elementName);

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
        
        await PopulateViewBag(elementName, action!, context, fieldName);

        return View(action!.GetType().Name, action);
        
    }

    private async Task<ComponentResult> GetIconSearchBoxResult(BasicAction? action)
    {
        var iconSearchBox = _searchBoxFactory.Create();
        iconSearchBox.DataItem.ShowIcon = true;
        iconSearchBox.DataItem.Items = Enum.GetValues<IconType>()
            .Select(i => new DataItemValue(i.GetId().ToString(), i.GetDescription(), i, "6a6a6a")).ToList();
        iconSearchBox.SelectedValue = ((int)action!.Icon).ToString();
        iconSearchBox.Name = "icon";

        var iconSearchBoxResult = await iconSearchBox.GetResultAsync();
        return iconSearchBoxResult;
    }

    public async Task<IActionResult> Add(string elementName, string actionType, ActionSource context, string? fieldName)
    {
        BasicAction action = actionType switch
        {
            nameof(ScriptAction) => new ScriptAction(),
            nameof(UrlRedirectAction) => new UrlRedirectAction(),
            nameof(InternalAction) => new InternalAction(),
            nameof(SqlCommandAction) => new SqlCommandAction(),
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
    
        var iconSearchBoxResult = await GetIconSearchBoxResult(action);

        if (iconSearchBoxResult.IsActionResult())
            return iconSearchBoxResult.ToActionResult();

        await PopulateViewBag(elementName, action, context);
        return View(action);
    }
    

    [HttpPost]
    public async Task<ActionResult> Remove(string elementName, string actionName, ActionSource context, string? fieldName)
    {
        await _actionsService.DeleteActionAsync(elementName, actionName, context, fieldName);
        return Json(new { success = true });
    }


    [HttpPost]
    public async Task<ActionResult> Sort(string elementName, string[] orderFields, ActionSource context, string? fieldName)
    {
        await _actionsService.SortActionsAsync(elementName, orderFields, context, fieldName);
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<ActionResult> EnableDisable(string elementName, string actionName, ActionSource context, bool value)
    {
        await _actionsService.EnableDisable(elementName, actionName, context, value);
        return Json(new { success = true });
    }


    [HttpPost]
    public async Task<IActionResult> InsertAction(string elementName, InsertAction insertAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,insertAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ConfigAction(string elementName, ConfigAction configAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,configAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ExportAction(string elementName, ExportAction exportAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,exportAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ViewAction(string elementName, ViewAction viewAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,viewAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> EditAction(string elementName, EditAction editAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,editAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAction(string elementName, DeleteAction deleteAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,deleteAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> ImportAction(string elementName, ImportAction importAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,importAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> RefreshAction(string elementName, RefreshAction refreshAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,refreshAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> LegendAction(string elementName, LegendAction legendAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,legendAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> SortAction(string elementName, SortAction sortAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,sortAction,context,isActionSave,originalName);
    }
    
    [HttpPost]
    public async Task<IActionResult> SaveAction(string elementName, SaveAction saveAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,saveAction,context,isActionSave,originalName);
    }
    
    [HttpPost]
    public async Task<IActionResult> CancelAction(string elementName, CancelAction cancelAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,cancelAction,context,isActionSave,originalName);
    }
    
    [HttpPost]
    public async Task<IActionResult> BackAction(string elementName, BackAction cancelAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,cancelAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> LogAction(string elementName, LogAction logAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,logAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> FilterAction(string elementName, FilterAction filterAction, ActionSource context, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,filterAction,context,isActionSave,originalName);
    }

    [HttpPost]
    public async Task<IActionResult> UrlRedirectAction(string elementName, UrlRedirectAction urlAction, ActionSource context,
        string? fieldName, string? originalName, bool isActionSave)
    {
        return await EditActionResult(elementName,urlAction,context,isActionSave,originalName,fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> ScriptAction(string elementName, ScriptAction scriptAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return await EditActionResult(elementName,scriptAction,context,isActionSave,originalName,fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> SqlCommandAction(string elementName, SqlCommandAction sqlAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return await EditActionResult(elementName,sqlAction,context,isActionSave,originalName,fieldName);
    }
    

    [HttpPost]
    public async Task<IActionResult> InternalAction(string elementName, InternalAction internalAction, ActionSource context,
        string? originalName, bool isActionSave, string? fieldName)
    {
        return await EditActionResult(elementName,internalAction,context,isActionSave,originalName,fieldName);
    }

    [HttpPost]
    public async Task<IActionResult> AddRelation(string elementName, InternalAction internalAction, ActionSource context,
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
    public async Task<IActionResult> RemoveRelation(string elementName, InternalAction internalAction, ActionSource context,
        int relationIndex, string? fieldName)
    {
        internalAction.ElementRedirect.RelationFields.RemoveAt(relationIndex);
        await PopulateViewBag(elementName, internalAction, context, fieldName);
        return View(internalAction.GetType().Name, internalAction);
    }

    private async Task SaveAction(string elementName, BasicAction basicAction, ActionSource context,string? originalName,string? fieldName = null)
    {
        await _actionsService.SaveAction(elementName, basicAction, context, originalName, fieldName);
        
        if (ModelState.IsValid)
            ViewBag.Success = true;
        else
            ViewBag.Error = _actionsService.GetValidationSummary().GetHtml();
    }

    private async Task PopulateViewBag(string elementName, BasicAction basicAction, ActionSource context, string? fieldName = null)
    {
        if ((string?)Request.Query["originalName"] != null)
            ViewBag.OriginalName = Request.Query["originalName"];
        else if (TempData["originalName"] != null)
            ViewBag.OriginalName = TempData["originalName"]!;
        else
            ViewBag.OriginalName = basicAction.Name;

        if (TempData.TryGetValue("error", out var value))
            ViewBag.Error = value!;

        ViewBag.ElementName = elementName;
        ViewBag.ContextAction = context;
        ViewBag.MenuId = "Actions";
        ViewBag.FieldName = fieldName!;

        switch (basicAction)
        {
            case SqlCommandAction _:
            case ImportAction _:
            case InsertAction _:
            {
                var formElement = await _actionsService.GetFormElementAsync(elementName);
                ViewBag.FormElement = formElement;
                break;
            }
            case ExportAction _:
            {
                var formElement = await _actionsService.GetFormElementAsync(elementName);
                ViewBag.HintDictionary = _actionsService.GetHintDictionary(formElement);
                break;
            }
            case InternalAction internalAction:
            {
                ViewBag.ElementNameList = _actionsService.GetElementListAsync();
                ViewBag.InternalFieldList = _actionsService.GetFieldList(elementName);
                string elementNameRedirect = internalAction.ElementRedirect.ElementNameRedirect;
                ViewBag.RedirectFieldList = _actionsService.GetFieldList(elementNameRedirect);
                break;
            }
        }
    }


}