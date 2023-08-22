using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class ActionsService : BaseService
{

    public ActionsService(
        IValidationDictionary validationDictionary, 
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IDataDictionaryRepository dataDictionaryRepository) 
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {

    }

    public async Task<bool> DeleteActionAsync(string elementName, string actionName, ActionSource context, string? fieldName = null)
    {
        var dicParser = await DataDictionaryRepository.GetMetadataAsync(elementName);
        DeleteAction(dicParser, actionName, context, fieldName);
        await DataDictionaryRepository.InsertOrReplaceAsync(dicParser);

        return true;
    }

    private void DeleteAction(FormElement formElement, string? originalName, ActionSource context, string? fieldName = null)
    {
        if (originalName == null)
            return;

        switch (context)
        {
            case ActionSource.Field:
            {
                var field = formElement.Fields[fieldName!];
                var fieldAction = field.Actions.Get(originalName);
                field.Actions.Remove(fieldAction);
                break;
            }
            case ActionSource.GridTable:
            {
                var gridTableAction = formElement.Options.GridTableActions.Get(originalName);
                formElement.Options.GridTableActions.Remove(gridTableAction);
                break;
            }
            case ActionSource.GridToolbar:
            {
                var gridToolbarAction = formElement.Options.GridToolbarActions.Get(originalName);
                formElement.Options.GridToolbarActions.Remove(gridToolbarAction);
                break;
            }
            case ActionSource.FormToolbar:
                var formToolbarAction = formElement.Options.FormToolbarActions.Get(originalName);
                formElement.Options.GridToolbarActions.Remove(formToolbarAction);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context, null);
        }
    }

    public async Task<bool> SaveAction(string elementName, BasicAction action, ActionSource context, string? originalName, string? fieldName = null)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        ValidateActionName(formElement, action.Name, originalName, context, fieldName);
        ValidateAction(formElement, action);

        if (!IsValid)
            return false;

        if (originalName != null && !originalName.Equals(action.Name))
        {
            DeleteAction(formElement, originalName, context, fieldName);
        }
                
        switch (context)
        {
            case ActionSource.Field:
            {
                var field = formElement.Fields[fieldName!];
                field.Actions.Set(action);

                if (action.IsDefaultOption)
                    field.Actions.SetDefaultOption(action.Name);
                break;
            }
            case ActionSource.GridTable:
            {
                formElement.Options.GridTableActions.Set(action);

                if (action.IsDefaultOption)
                    formElement.Options.GridTableActions.SetDefaultOption(action.Name);
                break;
            }
            case ActionSource.GridToolbar:
                formElement.Options.GridToolbarActions.Set(action);
                break;
            case ActionSource.FormToolbar:
                formElement.Options.FormToolbarActions.Set(action);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), context, null);
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return true;
    }

    private void ValidateActionName(FormElement formElement, string actionName, string? originalName, ActionSource context, string? fieldName = null)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            AddError(nameof(actionName), StringLocalizer["Required [Name] field"]);
        }

        if (originalName != null && originalName.Equals(actionName))
            return;
        
        IList<BasicAction>? listAction = null;
        switch (context)
        {
            case ActionSource.Field:
            {
                var field = formElement.Fields[fieldName!];
                listAction = field.Actions.GetAllSorted();
                break;
            }
            case ActionSource.GridTable:
                listAction = formElement.Options.GridTableActions.GetAllSorted();
                break;
            case ActionSource.GridToolbar:
                listAction = formElement.Options.GridToolbarActions.GetAllSorted();
                break;
        }

        if (listAction == null) 
            return;
        
        foreach (var a in listAction)
        {
            if (a.Name.Equals(actionName))
                AddError(nameof(actionName),
                    StringLocalizer["Invalid[Name] field! There is already an action registered with that name."]);
        }
    }

    private void ValidateAction(FormElement formElement, BasicAction action)
    {
        if (string.IsNullOrWhiteSpace(action.VisibleExpression))
            AddError(nameof(action.VisibleExpression), StringLocalizer["Required [VisibleExpression] field"]);
        else if (!ValidateExpression(action.VisibleExpression, "val:", "exp:", "sql:"))
            AddError(nameof(action.VisibleExpression), StringLocalizer["Invalid [VisibleExpression] field"]);

        if (string.IsNullOrWhiteSpace(action.EnableExpression))
            AddError(nameof(action.EnableExpression), "Required [EnableExpression] field");
        else if (!ValidateExpression(action.EnableExpression, "val:", "exp:", "sql:"))
            AddError(nameof(action.EnableExpression), "Invalid [EnableExpression] field");

        if (action is SqlCommandAction sqlAction)
        {
            if (string.IsNullOrEmpty(sqlAction.CommandSql))
                AddError(nameof(sqlAction.CommandSql), StringLocalizer["Required [Sql Command] field"]);

            if (!formElement.Options.Grid.EnableMultSelect && sqlAction.ApplyOnSelected)
                AddError(nameof(sqlAction.ApplyOnSelected), StringLocalizer["[Apply On Selected] field can only be enabled if the EnableMultSelect option of the grid is enabled"]);
        }

        else if (action is UrlRedirectAction urlAction)
        {
            if (string.IsNullOrEmpty(urlAction.UrlRedirect))
                AddError(nameof(urlAction.UrlRedirect), StringLocalizer["Required [Url Redirect] field"]);
        }
        else if (action is ScriptAction scriptAction)
        {
            if (string.IsNullOrEmpty(scriptAction.OnClientClick))
                AddError(nameof(scriptAction.OnClientClick), StringLocalizer["Required [JavaScript Command] field"]);
        }
        else if (action is InternalAction internalAction)
        {
            if (string.IsNullOrEmpty(internalAction.ElementRedirect.ElementNameRedirect))
                AddError(nameof(internalAction.ElementRedirect.ElementNameRedirect), StringLocalizer["Required [Entity Name] field"]);
        }
    }

    public async Task<bool> SortActionsAsync(string elementName, string[] listAction, ActionSource actionContext, string? fieldName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
        for (int i = 0; i < listAction.Length; i++)
        {
            string actionName = listAction[i];

            var action = actionContext switch
            {
                ActionSource.GridTable => formElement.Options.GridTableActions.Get(actionName),
                ActionSource.GridToolbar => formElement.Options.GridToolbarActions.Get(actionName),
                ActionSource.Field => formElement.Fields[fieldName!].Actions.Get(actionName),
                ActionSource.FormToolbar => formElement.Options.FormToolbarActions.Get(actionName),
                _ => throw new ArgumentOutOfRangeException(nameof(actionContext), actionContext, null)
            };
            action.Order = i + 1;
        }
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return true;
    }

    public async Task<bool> EnableDisable(string elementName, string actionName, ActionSource actionContext, bool visible)
    {
        var dicParser = await DataDictionaryRepository.GetMetadataAsync(elementName);
        var action = actionContext switch
        {
            ActionSource.GridTable => dicParser.Options.GridTableActions.Get(actionName),
            ActionSource.GridToolbar => dicParser.Options.GridToolbarActions.Get(actionName),
            ActionSource.FormToolbar => dicParser.Options.FormToolbarActions.Get(actionName),
            _ => throw new JJMasterDataException("Invalid ActionSource.")
        };

        action!.SetVisible(visible);
        await DataDictionaryRepository.InsertOrReplaceAsync(dicParser);

        return true;
    }

    public async Task<Dictionary<string, string>> GetFieldList(string elementName)
    {
        var dicFields = new Dictionary<string, string> { { string.Empty, StringLocalizer["--Select--"] } };

        if (string.IsNullOrEmpty(elementName))
            return dicFields;

        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

        foreach (var field in formElement.Fields)
        {
            dicFields.Add(field.Name, field.Name);
        }

        return dicFields;
    }

}