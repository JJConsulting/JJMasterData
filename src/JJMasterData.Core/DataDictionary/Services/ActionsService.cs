using System.Collections.Generic;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Services;

public class ActionsService : BaseService
{
    public ActionsService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository) 
        : base(validationDictionary, dictionaryRepository)
    {
    }

    public bool DeleteAction(string elementName, string actionName, ActionOrigin context, string fieldName = null)
    {
        var dicParser = DictionaryRepository.GetMetadata(elementName);
        DeleteAction(ref dicParser, actionName, context, fieldName);
        DictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    private void DeleteAction(ref Metadata metadata, string originalName, ActionOrigin context, string fieldName = null)
    {
        if (originalName == null)
            return;

        if (context == ActionOrigin.Field)
        {
            var field = metadata.GetFormElement().FormFields[fieldName];
            var action = field.Actions.Get(originalName);
            if (action != null)
                field.Actions.Remove(action);
        }
        else if (context == ActionOrigin.Grid)
        {
            var action = metadata.UIOptions.GridActions.Get(originalName);
            if (action != null)
                metadata.UIOptions.GridActions.Remove(action);
        }
        else if (context == ActionOrigin.Toolbar)
        {
            var action = metadata.UIOptions.ToolBarActions.Get(originalName);
            if (action != null)
                metadata.UIOptions.ToolBarActions.Remove(action);
        }
    }

    public bool SaveAction(string elementName, BasicAction action, ActionOrigin context, string originalName, string fieldName = null)
    {
        var dicParser = DictionaryRepository.GetMetadata(elementName);
        ValidateActionName(dicParser, action.Name, originalName, context, fieldName);
        ValidateAction(dicParser, action);

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
                DeleteAction(ref dicParser, originalName, context, fieldName);
            }
        }
                
        switch (context)
        {
            case ActionOrigin.Field:
            {
                var field = dicParser.GetFormElement().FormFields[fieldName];
                field.Actions.Set(action);

                if (action.IsDefaultOption)
                    field.Actions.SetDefault(action.Name);
                break;
            }
            case ActionOrigin.Grid:
            {
                dicParser.UIOptions.GridActions.Set(action);

                if (action.IsDefaultOption)
                    dicParser.UIOptions.GridActions.SetDefault(action.Name);
                break;
            }
            case ActionOrigin.Toolbar:
                dicParser.UIOptions.ToolBarActions.Set(action);
                break;
        }

        DictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    private void ValidateActionName(Metadata dicParser, string actionName, string originalName, ActionOrigin context, string fieldName = null)
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
            case ActionOrigin.Field:
            {
                var field = dicParser.GetFormElement().FormFields[fieldName];
                listAction = field.Actions.GetAll();
                break;
            }
            case ActionOrigin.Grid:
                listAction = dicParser.UIOptions.GridActions.GetAll();
                break;
            case ActionOrigin.Toolbar:
                listAction = dicParser.UIOptions.ToolBarActions.GetAll();
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

    private void ValidateAction(Metadata dicParser, BasicAction action)
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

            if (!dicParser.UIOptions.Grid.EnableMultSelect && sqlAction.ApplyOnSelected)
                AddError(nameof(sqlAction.ApplyOnSelected), Translate.Key("[Apply On Selected] field can only be enabled if the EnableMultSelect option of the grid is enabled"));
        }

        if (action is PythonScriptAction pythonScriptAction)
        {
            if (string.IsNullOrEmpty(pythonScriptAction.PythonScript))
                AddError(nameof(pythonScriptAction.PythonScript), Translate.Key("Required [Python Script] field"));

            if (!dicParser.UIOptions.Grid.EnableMultSelect && pythonScriptAction.ApplyOnSelected)
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

    public bool SortActions(string elementName, string[] listAction, ActionOrigin actionContext, string fieldName)
    {
        var dicParser = DictionaryRepository.GetMetadata(elementName);
        for (int i = 0; i < listAction.Length; i++)
        {
            string actionName = listAction[i];
            BasicAction action = null;

            if (actionContext == ActionOrigin.Grid)
            {
                action = dicParser.UIOptions.GridActions.Get(actionName);
            }
            else if (actionContext == ActionOrigin.Toolbar)
            {
                action = dicParser.UIOptions.ToolBarActions.Get(actionName);
            }
            else if (actionContext == ActionOrigin.Field)
            {
                var formElement = dicParser.GetFormElement();
                action = formElement.FormFields[fieldName].Actions.Get(actionName);
            }
            action.Order = i + 1;
        }
        DictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    public bool EnableDisable(string elementName, string actionName, ActionOrigin actionContext, bool visible)
    {
        var dicParser = DictionaryRepository.GetMetadata(elementName);
        BasicAction action = null;
        if (actionContext == ActionOrigin.Grid)
        {
            action = dicParser.UIOptions.GridActions.Get(actionName);
        }
        else if (actionContext == ActionOrigin.Toolbar)
        {
            action = dicParser.UIOptions.ToolBarActions.Get(actionName);
        }

        action.SetVisible(visible);
        DictionaryRepository.InsertOrReplace(dicParser);

        return true;
    }

    public Dictionary<string, string> GetFieldList(string elementName)
    {
        var dicFields = new Dictionary<string, string>();
        dicFields.Add(string.Empty, Translate.Key("--Select--"));

        if (string.IsNullOrEmpty(elementName))
            return dicFields;

        var dataEntry = DictionaryRepository.GetMetadata(elementName);
        if (dataEntry == null)
            return dicFields;

        foreach (var field in dataEntry.Table.Fields)
        {
            dicFields.Add(field.Name, field.Name);
        }

        return dicFields;
    }

}