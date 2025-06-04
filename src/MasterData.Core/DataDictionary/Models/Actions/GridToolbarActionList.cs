using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridToolbarActionList : FormElementActionList
{
    [JsonPropertyName("insertAction")]
    public InsertAction InsertAction
    {
        get => GetOrAdd<InsertAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("legendAction")]
    public LegendAction LegendAction
    {
        get => GetOrAdd<LegendAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("refreshAction")]
    public RefreshAction RefreshAction
    {
        get => GetOrAdd<RefreshAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("filterAction")]
    public FilterAction FilterAction
    {
        get => GetOrAdd<FilterAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("importAction")]
    public ImportAction ImportAction
    {
        get => GetOrAdd<ImportAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("exportAction")]
    public ExportAction ExportAction
    {
        get => GetOrAdd<ExportAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("configAction")]
    public ConfigAction ConfigAction
    {
        get => GetOrAdd<ConfigAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("sortAction")]
    public SortAction SortAction
    {
        get => GetOrAdd<SortAction>();
        set => SetOfType(value);
    }

    [JsonPropertyName("auditLogGridToolbarAction")]
    public AuditLogGridToolbarAction AuditLogGridToolbarAction
    {
        get => GetOrAdd<AuditLogGridToolbarAction>();
        set => SetOfType(value);
    }

    public GridToolbarActionList()
    {
        Add(new InsertAction());
        Add(new LegendAction());
        Add(new RefreshAction());
        Add(new FilterAction());
        Add(new ImportAction());
        Add(new ExportAction());
        Add(new ConfigAction());
        Add(new SortAction());
        Add(new AuditLogGridToolbarAction());
    }

    public GridToolbarActionList(List<BasicAction> list) : base(list)
    {
    }

    public GridToolbarActionList DeepCopy()
    {
        return new GridToolbarActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}
