using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ActionsService : BaseService
{
    public ActionsService(IValidationDictionary validationDictionary, IDataDictionaryRepository dataDictionaryRepository) 
        : base(validationDictionary, dataDictionaryRepository)
    {
    }

    public bool DeleteAction(string elementName, string actionName, ActionSource context, string fieldName = null)
    {
        var dicParser = DataDictionaryRepository.GetMetadata(elementName);
        DeleteAction(dicParser, actionName, context, fieldName);
        DataDictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    private void DeleteAction(FormElement formElement, string originalName, ActionSource context, string fieldName = null)
    {
        if (originalName == null)
            return;

        if (context == ActionSource.Field)
        {
            var field = formElement.Fields[fieldName];
            var action = field.Actions.Get(originalName);
            if (action != null)
                field.Actions.Remove(action);
        }
        else if (context == ActionSource.Grid)
        {
            var action = formElement.Options.GridActions.Get(originalName);
            if (action != null)
                formElement.Options.GridActions.Remove(action);
        }
        else if (context == ActionSource.Toolbar)
        {
            var action = formElement.Options.ToolBarActions.Get(originalName);
            if (action != null)
                formElement.Options.ToolBarActions.Remove(action);
        }
    }

    public bool SaveAction(string elementName, BasicAction action, ActionSource context, string originalName, string fieldName = null)
    {
        var formElement = DataDictionaryRepository.GetMetadata(elementName);
        ValidateActionName(formElement, action.Name, originalName, context, fieldName);
        ValidateAction(formElement, action);

        if (!IsValid)
            return false;

        if (originalName != null && !originalName.Equals(action.Name))
        {
            if (action is UrlRedirectAction |
                action is ScriptAction | 
                action is SqlCommandAction |
                action is PythonScriptAction |
                action is InternalAction)
            {
                DeleteAction(formElement, originalName, context, fieldName);
            }
        }
                
        switch (context)
        {
            case ActionSource.Field:
            {
                var field = formElement.Fields[fieldName];
                field.Actions.Set(action);

                if (action.IsDefaultOption)
                    field.Actions.SetDefault(action.Name);
                break;
            }
            case ActionSource.Grid:
            {
                formElement.Options.GridActions.Set(action);

                if (action.IsDefaultOption)
                    formElement.Options.GridActions.SetDefault(action.Name);
                break;
            }
            case ActionSource.Toolbar:
                formElement.Options.ToolBarActions.Set(action);
                break;
        }

        DataDictionaryRepository.InsertOrReplace(formElement);

        return true;
    }

    private void ValidateActionName(FormElement dicParser, string actionName, string originalName, ActionSource context, string fieldName = null)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            AddError(nameof(actionName), Translate.Key("Required [Name] field"));
        }

        if (originalName != null && originalName.Equals(actionName))
            return;
        
        List<BasicAction> listAction = null;
        switch (context)
        {
            case ActionSource.Field:
            {
                var field = dicParser.Fields[fieldName];
                listAction = field.Actions.GetAll();
                break;
            }
            case ActionSource.Grid:
                listAction = dicParser.Options.GridActions.GetAll();
                break;
            case ActionSource.Toolbar:
                listAction = dicParser.Options.ToolBarActions.GetAll();
                break;
        }

        if (listAction == null) 
            return;
        
        foreach (var a in listAction)
        {
            if (a.Name.Equals(actionName))
                AddError(nameof(actionName),
                    Translate.Key("Invalid[Name] field! There is already an action registered with that name."));
        }
    }

    private void ValidateAction(FormElement formElement, BasicAction action)
    {
        if (string.IsNullOrWhiteSpace(action.VisibleExpression))
            AddError(nameof(action.VisibleExpression), Translate.Key("Required [VisibleExpression] field"));
        else if (!ValidateExpression(action.VisibleExpression, "val:", "exp:", "sql:"))
            AddError(nameof(action.VisibleExpression), Translate.Key("Invalid [VisibleExpression] field"));

        if (string.IsNullOrWhiteSpace(action.EnableExpression))
            AddError(nameof(action.EnableExpression), "Required [EnableExpression] field");
        else if (!ValidateExpression(action.EnableExpression, "val:", "exp:", "sql:"))
            AddError(nameof(action.EnableExpression), "Invalid [EnableExpression] field");

        if (action is SqlCommandAction sqlAction)
        {
            if (string.IsNullOrEmpty(sqlAction.CommandSQL))
                AddError(nameof(sqlAction.CommandSQL), Translate.Key("Required [Sql Command] field"));

            if (!formElement.Options.Grid.EnableMultSelect && sqlAction.ApplyOnSelected)
                AddError(nameof(sqlAction.ApplyOnSelected), Translate.Key("[Apply On Selected] field can only be enabled if the EnableMultSelect option of the grid is enabled"));
        }

        if (action is PythonScriptAction pythonScriptAction)
        {
            if (string.IsNullOrEmpty(pythonScriptAction.PythonScript))
                AddError(nameof(pythonScriptAction.PythonScript), Translate.Key("Required [Python Script] field"));

            if (!formElement.Options.Grid.EnableMultSelect && pythonScriptAction.ApplyOnSelected)
                AddError(nameof(pythonScriptAction.ApplyOnSelected), Translate.Key("[Apply On Selected] field can only be enabled if the EnableMultSelect option of the grid is enabled"));
        }

        else if (action is UrlRedirectAction urlAction)
        {
            if (string.IsNullOrEmpty(urlAction.UrlRedirect))
                AddError(nameof(urlAction.UrlRedirect), Translate.Key("Required [Url Redirect] field"));
        }
        else if (action is ScriptAction scriptAction)
        {
            if (string.IsNullOrEmpty(scriptAction.OnClientClick))
                AddError(nameof(scriptAction.OnClientClick), Translate.Key("Required [JavaScript Command] field"));
        }
        else if (action is InternalAction internalAction)
        {
            if (string.IsNullOrEmpty(internalAction.ElementRedirect.ElementNameRedirect))
                AddError(nameof(internalAction.ElementRedirect.ElementNameRedirect), Translate.Key("Required [Entity Name] field"));
        }
    }

    public bool SortActions(string elementName, string[] listAction, ActionSource actionContext, string fieldName)
    {
        var dicParser = DataDictionaryRepository.GetMetadata(elementName);
        for (int i = 0; i < listAction.Length; i++)
        {
            string actionName = listAction[i];
            BasicAction action = null;

            if (actionContext == ActionSource.Grid)
            {
                action = dicParser.Options.GridActions.Get(actionName);
            }
            else if (actionContext == ActionSource.Toolbar)
            {
                action = dicParser.Options.ToolBarActions.Get(actionName);
            }
            else if (actionContext == ActionSource.Field)
            {
                var formElement = dicParser;
                action = formElement.Fields[fieldName].Actions.Get(actionName);
            }
            action.Order = i + 1;
        }
        DataDictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    public bool EnableDisable(string elementName, string actionName, ActionSource actionContext, bool visible)
    {
        var dicParser = DataDictionaryRepository.GetMetadata(elementName);
        BasicAction action = null;
        if (actionContext == ActionSource.Grid)
        {
            action = dicParser.Options.GridActions.Get(actionName);
        }
        else if (actionContext == ActionSource.Toolbar)
        {
            action = dicParser.Options.ToolBarActions.Get(actionName);
        }

        action.SetVisible(visible);
        DataDictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    public Dictionary<string, string> GetFieldList(string elementName)
    {
        var dicFields = new Dictionary<string, string>();
        dicFields.Add(string.Empty, Translate.Key("--Select--"));

        if (string.IsNullOrEmpty(elementName))
            return dicFields;

        var formElement = DataDictionaryRepository.GetMetadata(elementName);
        if (formElement == null)
            return dicFields;

        foreach (var field in formElement.Fields)
        {
            dicFields.Add(field.Name, field.Name);
        }

        return dicFields;
    }

}