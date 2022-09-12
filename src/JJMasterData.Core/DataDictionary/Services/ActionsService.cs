using System.Collections.Generic;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Services;

public class ActionsService : BaseService
{
    public ActionsService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public bool DeleteAction(string elementName, string actionName, ActionOrigin context, string fieldName = null)
    {
        var dicParser = DicDao.GetDictionary(elementName);
        DeleteAction(ref dicParser, actionName, context, fieldName);
        DicDao.SetDictionary(dicParser);

        return true;
    }

    private void DeleteAction(ref DicParser dicParser, string originalName, ActionOrigin context, string fieldName = null)
    {
        if (originalName == null)
            return;

        if (context == ActionOrigin.Field)
        {
            var field = dicParser.GetFormElement().Fields[fieldName];
            var ac = field.Actions.Get(originalName);
            if (ac != null)
                field.Actions.Remove(ac);
        }
        else if (context == ActionOrigin.Grid)
        {
            var ac = dicParser.UIOptions.GridActions.Get(originalName);
            if (ac != null)
                dicParser.UIOptions.GridActions.Remove(ac);
        }
        else if (context == ActionOrigin.Toolbar)
        {
            var ac = dicParser.UIOptions.ToolBarActions.Get(originalName);
            if (ac != null)
                dicParser.UIOptions.ToolBarActions.Remove(ac);
        }
    }

    public bool SaveAction(string elementName, BasicAction action, ActionOrigin context, string originalName, string fieldName = null)
    {
        var dicParser = DicDao.GetDictionary(elementName);
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
                var field = dicParser.GetFormElement().Fields[fieldName];
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

        DicDao.SetDictionary(dicParser);

        return true;
    }

    private bool ValidateActionName(DicParser dicParser, string actionName, string originalName, ActionOrigin context, string fieldName = null)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            AddError(nameof(actionName), Translate.Key("Required [Name] field"));
            return false;
        }

        if (originalName != null && originalName.Equals(actionName))
            return true;


        List<BasicAction> listAction = null;
        if (context == ActionOrigin.Field)
        {
            var field = dicParser.GetFormElement().Fields[fieldName];
            listAction = field.Actions.GetAll();
        }
        else if (context == ActionOrigin.Grid)
        {
            listAction = dicParser.UIOptions.GridActions.GetAll();
        }
        else if (context == ActionOrigin.Toolbar)
        {
            listAction = dicParser.UIOptions.ToolBarActions.GetAll();
        }

        foreach (var a in listAction)
        {
            if (a.Name.Equals(actionName))
                AddError(nameof(actionName), Translate.Key("Invalid[Name] field! There is already an action registered with that name."));
        }

        return IsValid;
    }

    private bool ValidateAction(DicParser dicParser, BasicAction action)
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

        return IsValid;
    }

    public bool SortActions(string elementName, string[] listAction, ActionOrigin actionContext, string fieldName)
    {
        var dicParser = DicDao.GetDictionary(elementName);
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
                action = formElement.Fields[fieldName].Actions.Get(actionName);
            }
            action.Order = i + 1;
        }
        DicDao.SetDictionary(dicParser);

        return true;
    }

    public bool EnableDisable(string elementName, string actionName, ActionOrigin actionContext, bool visible)
    {
        var dicParser = DicDao.GetDictionary(elementName);
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
        DicDao.SetDictionary(dicParser);

        return true;
    }

    public Dictionary<string, string> GetFieldList(string elementName)
    {
        var dicFields = new Dictionary<string, string>();
        dicFields.Add(string.Empty, Translate.Key("--Select--"));

        if (string.IsNullOrEmpty(elementName))
            return dicFields;

        var dataEntry = DicDao.GetDictionary(elementName);
        if (dataEntry == null)
            return dicFields;

        foreach (var field in dataEntry.Table.Fields)
        {
            dicFields.Add(field.Name, field.Name);
        }

        return dicFields;
    }

}