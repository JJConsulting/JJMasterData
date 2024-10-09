#nullable disable

using JJMasterData.Core.DataDictionary.Models.Actions;
using Newtonsoft.Json;

namespace JJMasterData.LegacyMetadataMigrator.FormElementMigration;

public class GridToolbarActions
{
    [JsonProperty("insertAction")]
    public InsertAction InsertAction { get; set; } = new();

    [JsonProperty("legendAction")]
    public LegendAction LegendAction { get; set; } = new();

    [JsonProperty("refreshAction")]
    public RefreshAction RefreshAction { get; set; } = new();

    [JsonProperty("filterAction")]
    public FilterAction FilterAction { get; set; } = new();

    [JsonProperty("importAction")]
    public ImportAction ImportAction { get; set; } = new();

    [JsonProperty("exportAction")]
    public ExportAction ExportAction { get; set; } = new();

    [JsonProperty("configAction")]
    public ConfigAction ConfigAction { get; set; } = new();

    [JsonProperty("sortAction")]
    public SortAction SortAction { get; set; } = new();

    [JsonProperty("logAction")]
    public AuditLogGridToolbarAction AuditLogGridToolbarAction { get; set; } = new();

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
        switch (action)
        {
            case InsertAction insertAction:
                InsertAction = insertAction;
                break;
            case LegendAction legendAction:
                LegendAction = legendAction;
                break;
            case RefreshAction refreshAction:
                RefreshAction = refreshAction;
                break;
            case FilterAction filterAction:
                FilterAction = filterAction;
                break;
            case ImportAction importAction:
                ImportAction = importAction;
                break;
            case ExportAction exportAction:
                ExportAction = exportAction;
                break;
            case ConfigAction configAction:
                ConfigAction = configAction;
                break;
            case SortAction sortAction:
                SortAction = sortAction;
                break;
            case AuditLogGridToolbarAction logAction:
                AuditLogGridToolbarAction = logAction;
                break;
            case SqlCommandAction cmdAction:
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
                break;
            }
            case UrlRedirectAction urlAction:
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
                break;
            }
            case InternalAction internalAction:
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
                break;
            }
            case ScriptAction scriptAction:
            {
                for (int i = 0; i < JsActions.Count; i++)
                {
                    if (JsActions[i].Name.Equals(action.Name))
                    {
                        JsActions[i] = scriptAction;
                        return;
                    }
                }
                JsActions.Add(scriptAction);
                break;
            }
            default:
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

    public void Add(BasicAction action)
    {
        if (action is SqlCommandAction cmdAction)
            Add(cmdAction);
        else if (action is UrlRedirectAction urlAction)
            Add(urlAction);
        else if (action is InternalAction internalAction)
            Add(internalAction);
        else if (action is ScriptAction scriptAction)
            Add(scriptAction);
        else
            throw new ArgumentException("Invalid Action");
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
        else if (action is ScriptAction jScriptAction)
        {
            Remove(jScriptAction);
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

    private void ValidateAction(BasicAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException("Property name action is not valid");
    }

    public BasicAction Get(string name)
    {
        return GetAll().Find(x => x.Name.Equals(name));
    }


    public List<BasicAction> GetAll()
    {
        var actionList = new List<BasicAction>();

        if (InsertAction is not null)
        {
        }
        else
        {
            InsertAction = new InsertAction();
        }

        actionList.Add(InsertAction);

        if (LegendAction is not null)
        {
        }
        else
        {
            LegendAction = new LegendAction();
        }

        actionList.Add(LegendAction);

        RefreshAction ??= new RefreshAction();

        actionList.Add(RefreshAction);

        FilterAction ??= new FilterAction();

        actionList.Add(FilterAction);

        ImportAction ??= new ImportAction();

        actionList.Add(ImportAction);

        ExportAction ??= new ExportAction();

        actionList.Add(ExportAction);

        ConfigAction ??= new ConfigAction();

        actionList.Add(ConfigAction);

        SortAction ??= new SortAction();

        actionList.Add(SortAction);

        AuditLogGridToolbarAction ??= new AuditLogGridToolbarAction();

        actionList.Add(AuditLogGridToolbarAction);
        if (CommandActions is { Count: > 0 })
            actionList.AddRange(CommandActions.ToArray());

        if (UrlRedirectActions is { Count: > 0 })
            actionList.AddRange(UrlRedirectActions.ToArray());

        if (InternalActions is { Count: > 0 })
            actionList.AddRange(InternalActions.ToArray());

        if (JsActions is { Count: > 0 })
            actionList.AddRange(JsActions.ToArray());
        return actionList.OrderBy(x => x.Order).ToList();
    }
}