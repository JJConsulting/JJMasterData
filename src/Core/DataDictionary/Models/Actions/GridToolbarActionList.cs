using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridToolbarActionList : FormElementActionList
{
    private InsertAction _insertAction = new();
    private LegendAction _legendAction = new();
    private RefreshAction _refreshAction = new();
    private FilterAction _filterAction = new();
    private ImportAction _importAction = new();
    private ExportAction _exportAction = new();
    private ConfigAction _configAction = new();
    private SortAction _sortAction = new();
    private AuditLogGridToolbarAction _auditLogGridToolbarAction = new();

    [JsonPropertyName("insertAction")]
    public InsertAction InsertAction
    {
        get => (InsertAction)List.Find(a => a is InsertAction) ?? _insertAction;
        set
        {
            _insertAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("legendAction")]
    public LegendAction LegendAction
    {
        get => (LegendAction)List.Find(a => a is LegendAction) ?? _legendAction;
        set
        {
            _legendAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("refreshAction")]
    public RefreshAction RefreshAction
    {
        get => (RefreshAction)List.Find(a => a is RefreshAction) ?? _refreshAction;
        set
        {
            _refreshAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("filterAction")]
    public FilterAction FilterAction
    {
        get => (FilterAction)List.Find(a => a is FilterAction) ?? _filterAction;
        set
        {
            _filterAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("importAction")]
    public ImportAction ImportAction
    {
        get => (ImportAction)List.Find(a => a is ImportAction) ?? _importAction;
        set
        {
            _importAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("exportAction")]
    public ExportAction ExportAction
    {
        get => (ExportAction)List.Find(a => a is ExportAction) ?? _exportAction;
        set
        {
            _exportAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("configAction")]
    public ConfigAction ConfigAction
    {
        get => (ConfigAction)List.Find(a => a is ConfigAction) ?? _configAction;
        set
        {
            _configAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("sortAction")]
    public SortAction SortAction
    {
        get => (SortAction)List.Find(a => a is SortAction) ?? _sortAction;
        set
        {
            _sortAction = value;
            Set(value);
        }
    }

    [JsonPropertyName("auditLogGridToolbarAction")]
    public AuditLogGridToolbarAction AuditLogGridToolbarAction
    {
        get => (AuditLogGridToolbarAction)List.Find(a => a is AuditLogGridToolbarAction) ?? _auditLogGridToolbarAction;
        set
        {
            _auditLogGridToolbarAction = value;
            Set(value);
        }
    }

    public GridToolbarActionList()
    {
    }

    public GridToolbarActionList(List<BasicAction> list) : base(list)
    {
    }

    public GridToolbarActionList DeepCopy()
    {
        return new GridToolbarActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}
