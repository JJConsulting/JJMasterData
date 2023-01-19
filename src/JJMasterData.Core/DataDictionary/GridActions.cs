using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class GridActions
{
    [DataMember(Name = "editAction")]
    public EditAction EditAction { get; private set; }

    [DataMember(Name = "deleteAction")]
    public DeleteAction DeleteAction { get; private set; }

    [DataMember(Name = "viewAction")]
    public ViewAction ViewAction { get; private set; }

    [DataMember(Name = "commandActions")]
    private List<SqlCommandAction> CommandActions { get; set; }

    [DataMember(Name = "pythonActions")]
    internal List<PythonScriptAction> PythonActions { get; set; }

    [DataMember(Name = "urlRedirectActions")]
    private List<UrlRedirectAction> UrlRedirectActions { get; set; }

    [DataMember(Name = "internalActions")]
    private List<InternalAction> InternalActions { get; set; }

    public GridActions()
    {
        ViewAction = new ViewAction();
        EditAction = new EditAction();
        DeleteAction = new DeleteAction();
        CommandActions = new List<SqlCommandAction>();
        UrlRedirectActions = new List<UrlRedirectAction>();
        InternalActions = new List<InternalAction>();
        PythonActions = new List<PythonScriptAction>();
    }

    public void Set(BasicAction action)
    {
        if (action is ViewAction viewAction)
        {
            ViewAction = viewAction;
        }
        else if (action is EditAction editAction)
        {
            EditAction = editAction;
        }
        else if (action is DeleteAction deleteAction)
        {
            DeleteAction = deleteAction;
        }
        else if (action is SqlCommandAction cmdAction)
        {
            for (int i = 0; i < CommandActions.Count; i++)
            {
                if (CommandActions[i].Name.Equals(action.Name))
                {
                    CommandActions[i] = cmdAction;
                    return;
                }
            }
            CommandActions.Add(cmdAction);
        }
        else if (action is PythonScriptAction pythonAction)
        {
            for (int i = 0; i < PythonActions.Count; i++)
            {
                if (PythonActions[i].Name.Equals(action.Name))
                {
                    PythonActions[i] = pythonAction;
                    return;
                }
            }
            PythonActions.Add(pythonAction);
        }
        else if (action is UrlRedirectAction urlAction)
        {
            for(int i =0;i< UrlRedirectActions.Count; i++)
            {
                if (UrlRedirectActions[i].Name.Equals(action.Name))
                {
                    UrlRedirectActions[i] = urlAction;
                    return;
                }
            }
            UrlRedirectActions.Add(urlAction);    
        }
        else if (action is InternalAction internalAction)
        {
            for (int i = 0; i < InternalActions.Count; i++)
            {
                if (InternalActions[i].Name.Equals(action.Name))
                {
                    InternalActions[i] = internalAction;
                    return;
                }
            }
            InternalActions.Add(internalAction);    
        }
        else
        {
            throw new ArgumentException(Translate.Key("Invalid Action"));
        }
    }

    public void Add(SqlCommandAction action)
    {
        ValidateAction(action);
        CommandActions.Add(action);
    }

    public void Add(PythonScriptAction action)
    {
        ValidateAction(action);
        PythonActions.Add(action);
    }

    public void Add(UrlRedirectAction action)
    {
        ValidateAction(action);
        UrlRedirectActions.Add(action);
    }

    public void Add(InternalAction action)
    {
        ValidateAction(action);
        InternalActions.Add(action);
    }

    public void Remove(SqlCommandAction action)
    {
        ValidateAction(action);
        CommandActions.Remove(action);
    }

    public void Remove(PythonScriptAction action)
    {
        ValidateAction(action);
        PythonActions.Remove(action);
    }

    public void Remove(UrlRedirectAction action)
    {
        ValidateAction(action);
        UrlRedirectActions.Remove(action);
    }

    public void Remove(InternalAction action)
    {
        ValidateAction(action);
        InternalActions.Remove(action);
    }

    public void Remove(BasicAction action)
    {
        if (action is SqlCommandAction acSql)
        {
            Remove(acSql);
        }
        else if (action is PythonScriptAction acPython)
        {
            Remove(acPython);
        }
        else if (action is UrlRedirectAction acUrl)
        {
            Remove(acUrl);
        }
        else if (action is InternalAction acInternal)
        {
            Remove(acInternal);
        }
        else
        {
            throw new ArgumentException(Translate.Key("Invalid Action"));
        }
    }

    public void Remove(string actionName)
    {
        BasicAction action = Get(actionName);
        Remove(action);
    }

    private void ValidateAction(BasicAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException(Translate.Key("Property name action is not valid"));
    }

    public BasicAction Get(string name)
    {
        BasicAction action = null;
        if (ViewAction.Name.Equals(name))
            return ViewAction;

        if (EditAction.Name.Equals(name))
            return EditAction;

        if (DeleteAction.Name.Equals(name))
            return DeleteAction;

        action = CommandActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        action = UrlRedirectActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        action = InternalActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        action = PythonActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        return null;
    }

    public void SetDefault(string actionName)
    {
        foreach (var action in GetAll())
        {
            action.IsDefaultOption = action.Name.Equals(actionName);
        }
    }

    public List<BasicAction> GetAll()
    {
        var listAction = new List<BasicAction>();

        if (ViewAction != null)
            listAction.Add(ViewAction);

        if (EditAction != null)
            listAction.Add(EditAction);

        if (DeleteAction != null)
            listAction.Add(DeleteAction);

        if (CommandActions != null && CommandActions.Count > 0)
            listAction.AddRange(CommandActions.ToArray());

        if (PythonActions != null && PythonActions.Count > 0)
            listAction.AddRange(PythonActions.ToArray());

        if (UrlRedirectActions != null && UrlRedirectActions.Count > 0)
            listAction.AddRange(UrlRedirectActions.ToArray());

        if (InternalActions != null && InternalActions.Count > 0)
            listAction.AddRange(InternalActions.ToArray());

        return listAction.OrderBy(x => x.Order).ToList();
    }

    public int Count
    {
        get { return GetAll().FindAll(x => x.IsVisible).Count; }
    }

}