using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormElementFieldActions
{
    [DataMember(Name = "commandActions")]
    private List<SqlCommandAction> CommandActions { get; set; }

    [DataMember(Name = "urlRedirectActions")]
    private List<UrlRedirectAction> UrlRedirectActions { get; set; }

    [DataMember(Name = "internalActions")]
    private List<InternalAction> InternalActions { get; set; }

    [DataMember(Name = "scriptActions")]
    private List<ScriptAction> ScriptActions { get; set; }

    public FormElementFieldActions()
    {
        CommandActions = new List<SqlCommandAction>();
        UrlRedirectActions = new List<UrlRedirectAction>();
        InternalActions = new List<InternalAction>();
        ScriptActions = new List<ScriptAction>();
    }


    public void Set(BasicAction action)
    {
        if (action is SqlCommandAction cmdAction)
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
        else if (action is UrlRedirectAction urlAction)
        {
            for (int i = 0; i < UrlRedirectActions.Count; i++)
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
        else if (action is ScriptAction scriptAction)
        {
            for (int i = 0; i < ScriptActions.Count; i++)
            {
                if (ScriptActions[i].Name.Equals(action.Name))
                {
                    ScriptActions[i] = scriptAction;
                    return;
                }
            }
            ScriptActions.Add(scriptAction);    
        }
        else
        {
            throw new ArgumentException(Translate.Key("Invalid Action"));
        }
    }


    public void Add(BasicAction action)
    {
        if (action is SqlCommandAction acSql)
        {
            Add(acSql);
        }
        else if (action is UrlRedirectAction acUrl)
        {
            Add(acUrl);
        }
        else if (action is InternalAction acInternal)
        {
            Add(acInternal);
        }
        else if (action is ScriptAction acScript)
        {
            Add(acScript);
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

    public void Add(ScriptAction action)
    {
        ValidateAction(action);
        if (ScriptActions == null)
            ScriptActions = new List<ScriptAction>();

        ScriptActions.Add(action);
    }

    public void Remove(BasicAction action)
    {
        ValidateAction(action);
        if (action is SqlCommandAction acSql)
        {
            CommandActions.Remove(acSql);
        }
        else if (action is UrlRedirectAction acUrl)
        {
            UrlRedirectActions.Remove(acUrl);
        }
        else if (action is InternalAction acInternal)
        {
            InternalActions.Remove(acInternal);
        }
        else if (action is ScriptAction acScript)
        {
            ScriptActions.Remove(acScript);
        }
        else
        {
            throw new ArgumentException(Translate.Key("Invalid Action"));
        }
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
        action = CommandActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        action = UrlRedirectActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        action = InternalActions.Find(x => x.Name.Equals(name));
        if (action != null)
            return action;

        if (ScriptActions != null)
        {
            action = ScriptActions.Find(x => x.Name.Equals(name));
            if (action != null)
                return action;
        }

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

        if (CommandActions != null && CommandActions.Count > 0)
            listAction.AddRange(CommandActions.ToArray());

        if (UrlRedirectActions != null && UrlRedirectActions.Count > 0)
            listAction.AddRange(UrlRedirectActions.ToArray());

        if (InternalActions != null && InternalActions.Count > 0)
            listAction.AddRange(InternalActions.ToArray());

        if (ScriptActions != null && ScriptActions.Count > 0)
            listAction.AddRange(ScriptActions.ToArray());

        return listAction.OrderBy(x => x.Order).ToList();
    }



}