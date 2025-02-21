#nullable disable

using JJMasterData.Core.DataDictionary.Models.Actions;
using Newtonsoft.Json;

namespace JJMasterData.LegacyMetadataMigrator.FormElementMigration;


public class GridActions
{
    [JsonProperty("editAction")]
    public EditAction EditAction { get; set; } = new();

    [JsonProperty("deleteAction")]
    public DeleteAction DeleteAction { get; set; } = new();

    [JsonProperty("viewAction")]
    public ViewAction ViewAction { get; set; } = new();

    [JsonProperty("commandActions")]
    private List<SqlCommandAction> CommandActions { get; set; } = [];

    [JsonProperty("urlRedirectActions")]
    private List<UrlRedirectAction> UrlRedirectActions { get; set; } = [];

    [JsonProperty("internalActions")]
    private List<InternalAction> InternalActions { get; set; } = [];

    [JsonProperty("jsActions")]
    private List<ScriptAction> JsActions { get; set; } = [];


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
        else if (action is ScriptAction jsAction)
        {
            for (int i = 0; i < JsActions.Count; i++)
            {
                if (JsActions[i].Name.Equals(action.Name))
                {
                    JsActions[i] = jsAction;
                    return;
                }
            }
            JsActions.Add(jsAction);    
        }
        else
        {
            throw new ArgumentException("Invalid Action");
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
        JsActions.Add(action);
    }

    public void Remove(SqlCommandAction action)
    {
        ValidateAction(action);
        CommandActions.Remove(action);
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
    
    public void Remove(ScriptAction action)
    {
        ValidateAction(action);
        JsActions.Remove(action);
    }


    public void Remove(BasicAction action)
    {
        if (action is SqlCommandAction acSql)
        {
            Remove(acSql);
        }
        else if (action is UrlRedirectAction acUrl)
        {
            Remove(acUrl);
        }
        else if (action is InternalAction acInternal)
        {
            Remove(acInternal);
        }
        else if (action is ScriptAction acJs)
        {
            Remove(acJs);
        }
        else
        {
            throw new ArgumentException("Invalid Action");
        }
    }

    public void Remove(string actionName)
    {
        BasicAction action = Get(actionName);
        Remove(action);
    }

#pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void ValidateAction(BasicAction action)
#pragma warning restore CA1822
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException("Property name action is not valid");
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

        action = JsActions.Find(x => x.Name.Equals(name));
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

        if (ViewAction is not  null)
            listAction.Add(ViewAction);

        if (EditAction is not null)
            listAction.Add(EditAction);

        if (DeleteAction is not  null)
            listAction.Add(DeleteAction);

        if (CommandActions is { Count: > 0 })
            listAction.AddRange(CommandActions.ToArray());
        
        if (UrlRedirectActions is { Count: > 0 })
            listAction.AddRange(UrlRedirectActions.ToArray());

        if (InternalActions is { Count: > 0 })
            listAction.AddRange(InternalActions.ToArray());

        if (JsActions is { Count: > 0 })
            listAction.AddRange(JsActions.ToArray());
        
        return listAction.OrderBy(x => x.Order).ToList();
    }

    public int Count => GetAll().FindAll(x => x.IsVisible).Count;
}