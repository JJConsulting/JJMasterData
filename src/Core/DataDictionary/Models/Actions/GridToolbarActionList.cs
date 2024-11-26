using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridToolbarActionList : FormElementActionList
{
    [JsonPropertyName("insertAction")]
    public InsertAction InsertAction
    {
        get => GetOrAdd<InsertAction>();
        set => Set(value);
    }

    [JsonPropertyName("legendAction")]
    public LegendAction LegendAction
    {
        get => GetOrAdd<LegendAction>();
        set => Set(value);
    }

    [JsonPropertyName("refreshAction")]
    public RefreshAction RefreshAction
    {
        get => GetOrAdd<RefreshAction>();
        set => Set(value);
    }

    [JsonPropertyName("filterAction")]
    public FilterAction FilterAction
    {
        get => GetOrAdd<FilterAction>();
        set => Set(value);
    }

    [JsonPropertyName("importAction")]
    public ImportAction ImportAction
    {
        get => GetOrAdd<ImportAction>();
        set => Set(value);
    }

    [JsonPropertyName("exportAction")]
    public ExportAction ExportAction
    {
        get => GetOrAdd<ExportAction>();
        set => Set(value);
    }

    [JsonPropertyName("configAction")]
    public ConfigAction ConfigAction
    {
        get => GetOrAdd<ConfigAction>();
        set => Set(value);
    }

    [JsonPropertyName("sortAction")]
    public SortAction SortAction
    {
        get => GetOrAdd<SortAction>();
        set => Set(value);
    }

    [JsonPropertyName("auditLogGridToolbarAction")]
    public AuditLogGridToolbarAction AuditLogGridToolbarAction
    {
        get => GetOrAdd<AuditLogGridToolbarAction>();
        set => Set(value);
    }

    public GridToolbarActionList()
    {
        Set(new InsertAction());
        Set(new LegendAction());
        Set(new RefreshAction());
        Set(new FilterAction());
        Set(new ImportAction());
        Set(new ExportAction());
        Set(new ConfigAction());
        Set(new SortAction());
        Set(new AuditLogGridToolbarAction());
    }

    public GridToolbarActionList(List<BasicAction> list) : base(list)
    {
    }

    public GridToolbarActionList DeepCopy()
    {
        return new GridToolbarActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}
