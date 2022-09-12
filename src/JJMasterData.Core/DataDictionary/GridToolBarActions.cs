using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class GridToolBarActions
{
    [DataMember(Name = "insertAction")]
    public InsertAction InsertAction { get; set; }

    [DataMember(Name = "legendAction")]
    public LegendAction LegendAction { get; set; }

    [DataMember(Name = "refreshAction")]
    public RefreshAction RefreshAction { get; set; }

    [DataMember(Name = "filterAction")]
    public FilterAction FilterAction { get; set; }

    [DataMember(Name = "importAction")]
    public ImportAction ImportAction { get; set; }

    [DataMember(Name = "exportAction")]
    public ExportAction ExportAction { get; set; }

    [DataMember(Name = "configAction")]
    public ConfigAction ConfigAction { get; set; }

    [DataMember(Name = "sortAction")]
    public SortAction SortAction { get; set; }

    [DataMember(Name = "logAction")]
    public LogAction LogAction { get; set; }

    [DataMember(Name = "commandActions")]
    private List<SqlCommandAction> CommandActions { get; set; }

    [DataMember(Name = "pythonActions")]
    internal List<PythonScriptAction> PythonActions { get; set; }

    [DataMember(Name = "urlRedirectActions")]
    private List<UrlRedirectAction> UrlRedirectActions { get; set; }

    [DataMember(Name = "internalActions")]
    private List<InternalAction> InternalActions { get; set; }

    public GridToolBarActions()
    {
        InsertAction = new InsertAction();
        LegendAction = new LegendAction();
        RefreshAction = new RefreshAction();
        FilterAction = new FilterAction();
        ImportAction = new ImportAction();
        ExportAction = new ExportAction();
        ConfigAction = new ConfigAction();
        SortAction = new SortAction();
        LogAction = new LogAction();
        CommandActions = new List<SqlCommandAction>();
        UrlRedirectActions = new List<UrlRedirectAction>();
        InternalActions = new List<InternalAction>();
        PythonActions = new List<PythonScriptAction>();
    }


    public void Set(BasicAction action)
    {
        if (action is InsertAction insertAction)
        {
            InsertAction = insertAction;
        }
        else if (action is LegendAction legendAction)
        {
            LegendAction = legendAction;
        }
        else if (action is RefreshAction refreshAction)
        {
            RefreshAction = refreshAction;
        }
        else if (action is FilterAction filterAction)
        {
            FilterAction = filterAction;
        }
        else if (action is ImportAction importAction)
        {
            ImportAction = importAction;
        }
        else if (action is ExportAction exportAction)
        {
            ExportAction = exportAction;
        }
        else if (action is ConfigAction configAction)
        {
            ConfigAction = configAction;
        }
        else if (action is SortAction sortAction)
        {
            SortAction = sortAction;
        }
        else if (action is LogAction logAction)
        {
            LogAction = logAction;
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

    public void Add(BasicAction action)
    {
        if (action is SqlCommandAction cmdAction)
            Add(cmdAction);
        else if (action is UrlRedirectAction urlAction)
            Add(urlAction);
        else if (action is InternalAction internalAction)
            Add(internalAction);
        else
            throw new ArgumentException(Translate.Key("Invalid Action"));
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
        else if (action is UrlRedirectAction acUrl)
        {
            Remove(acUrl);
        }
        else if (action is InternalAction acInternal)
        {
            Remove(acInternal);
        }
        else if (action is PythonScriptAction acPython)
        {
            Remove(acPython);
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
        return GetAll().Find(x => x.Name.Equals(name));
    }


    public List<BasicAction> GetAll()
    {
        var listAction = new List<BasicAction>();

        if (InsertAction != null)
        {
            listAction.Add(InsertAction);
        }
        else
        {
            InsertAction = new InsertAction();
            listAction.Add(InsertAction);
        }

        if (LegendAction != null)
        {
            listAction.Add(LegendAction);
        }
        else
        {
            LegendAction = new LegendAction();
            listAction.Add(LegendAction);
        }

        if (RefreshAction != null)
        {
            listAction.Add(RefreshAction);
        }
        else
        {
            RefreshAction = new RefreshAction();
            listAction.Add(RefreshAction);
        }

        if (FilterAction != null)
        {
            listAction.Add(FilterAction);
        }
        else
        {
            FilterAction = new FilterAction();
            listAction.Add(FilterAction);
        }

        if (ImportAction != null)
        {
            listAction.Add(ImportAction);
        }
        else
        {
            ImportAction = new ImportAction();
            listAction.Add(ImportAction);
        }

        if (ExportAction != null)
        {
            listAction.Add(ExportAction);
        }
        else
        {
            ExportAction = new ExportAction();
            listAction.Add(ExportAction);
        }

        if (ConfigAction != null)
        {
            listAction.Add(ConfigAction);
        }
        else
        {
            ConfigAction = new ConfigAction();
            listAction.Add(ConfigAction);
        }

        if (SortAction != null)
        {
            listAction.Add(SortAction);
        }
        else
        {
            SortAction = new SortAction();
            listAction.Add(SortAction);
        }

        if (LogAction != null)
        {
            listAction.Add(LogAction);
        }
        else
        {
            LogAction = new LogAction();
            listAction.Add(LogAction);
        }


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