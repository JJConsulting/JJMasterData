using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class GridToolbarActionList : FormElementActionList
{
    [JsonPropertyName("insertAction")]
    public InsertAction InsertAction { get; set; } = new();

    [JsonPropertyName("legendAction")]
    public LegendAction LegendAction { get; set; } = new();

    [JsonPropertyName("refreshAction")] 
    public RefreshAction RefreshAction { get; set; } = new();

    [JsonPropertyName("filterAction")]
    public FilterAction FilterAction { get; set; } = new();

    [JsonPropertyName("importAction")]
    public ImportAction ImportAction { get; set; } = new();

    [JsonPropertyName("exportAction")]
    public ExportAction ExportAction { get; set; } = new();

    [JsonPropertyName("configAction")]
    public ConfigAction ConfigAction { get; set; } = new();

    [JsonPropertyName("sortAction")]
    public SortAction SortAction { get; set; } = new();

    [JsonPropertyName("auditLogGridToolbarAction")]
    public AuditLogGridToolbarAction AuditLogGridToolbarAction { get; set; } = new();

    public GridToolbarActionList DeepCopy()
    {
        return new GridToolbarActionList
        {
            InsertAction = (InsertAction)InsertAction.DeepCopy(),
            LegendAction = (LegendAction)LegendAction.DeepCopy(),
            RefreshAction = (RefreshAction)RefreshAction.DeepCopy(),
            FilterAction = (FilterAction)FilterAction.DeepCopy(),
            ImportAction = (ImportAction)ImportAction.DeepCopy(),
            ExportAction = (ExportAction)ExportAction.DeepCopy(),
            ConfigAction = (ConfigAction)ConfigAction.DeepCopy(),
            SortAction = (SortAction)SortAction.DeepCopy(),
            AuditLogGridToolbarAction = (AuditLogGridToolbarAction)AuditLogGridToolbarAction.DeepCopy(),
            SqlActions = SqlActions.ConvertAll(action => (SqlCommandAction)action.DeepCopy()),
            UrlActions = UrlActions.ConvertAll(action => (UrlRedirectAction)action.DeepCopy()),
            HtmlTemplateActions = HtmlTemplateActions.ConvertAll(action => (HtmlTemplateAction)action.DeepCopy()),
            JsActions = JsActions.ConvertAll(action => (ScriptAction)action.DeepCopy()),
            PluginActions = PluginActions.ConvertAll(action => (PluginAction)action.DeepCopy())
        };
    }

    protected override IEnumerable<BasicAction> GetActions()
    {
        yield return InsertAction;
        yield return LegendAction;
        yield return RefreshAction;
        yield return FilterAction;
        yield return ImportAction;
        yield return ExportAction;
        yield return ConfigAction;
        yield return SortAction;
        yield return AuditLogGridToolbarAction;
    }
}