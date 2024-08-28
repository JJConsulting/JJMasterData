using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridToolbarActionList : FormElementActionList
{
    public InsertAction InsertAction { get; }
    public LegendAction LegendAction { get; }
    public RefreshAction RefreshAction { get; }
    public FilterAction FilterAction { get; }
    public ImportAction ImportAction { get; }
    public ExportAction ExportAction { get; }
    public ConfigAction ConfigAction { get; }
    public SortAction SortAction { get; }
    public AuditLogGridToolbarAction AuditLogGridToolbarAction { get; }

    public GridToolbarActionList()
    {
        InsertAction = new InsertAction();
        LegendAction = new LegendAction();
        RefreshAction = new RefreshAction();
        FilterAction = new FilterAction();
        ImportAction = new ImportAction();
        ExportAction = new ExportAction();
        ConfigAction = new ConfigAction();
        SortAction = new SortAction();
        AuditLogGridToolbarAction = new AuditLogGridToolbarAction();

        List.AddRange([
            InsertAction,
            LegendAction,
            RefreshAction,
            FilterAction,
            ImportAction,
            ExportAction,
            ConfigAction,
            SortAction,
            AuditLogGridToolbarAction
        ]);
    }

    [JsonConstructor]
    private GridToolbarActionList(List<BasicAction> list)
    {
        List = list;

        InsertAction = EnsureActionExists<InsertAction>();
        LegendAction = EnsureActionExists<LegendAction>();
        RefreshAction = EnsureActionExists<RefreshAction>();
        FilterAction = EnsureActionExists<FilterAction>();
        ImportAction = EnsureActionExists<ImportAction>();
        ExportAction = EnsureActionExists<ExportAction>();
        ConfigAction = EnsureActionExists<ConfigAction>();
        SortAction = EnsureActionExists<SortAction>();
        AuditLogGridToolbarAction = EnsureActionExists<AuditLogGridToolbarAction>();
    }

    public GridToolbarActionList DeepCopy()
    {
        return new GridToolbarActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}