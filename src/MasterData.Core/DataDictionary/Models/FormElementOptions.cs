#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJMasterData.Core.DataDictionary.Models.Actions;


namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementOptions
{
    [JsonPropertyName("gridOptions")]
    public GridUI Grid { get; set; } = new();

    [JsonPropertyName("formOptions")]
    public FormUI Form { get; set; } = new();

    [JsonPropertyName("toolBarActions")]
    [JsonConverter(typeof(GridToolbarActionListConverter))]
    public GridToolbarActionList GridToolbarActions { get; init;  } = [];

    [JsonPropertyName("formToolbarActions")]
    [JsonConverter(typeof(FormToolbarActionListConverter))]
    public FormToolbarActionList FormToolbarActions { get; init; } = [];

    [JsonPropertyName("gridActions")] 
    [JsonConverter(typeof(GridTableActionListConverter))]
    public GridTableActionList GridTableActions { get; init; } = [];

    [JsonPropertyName("enableAuditLog")]
    [Display(Name = "Enable Audit Log")]
    public bool EnableAuditLog { get; set; }
    
    [JsonPropertyName("useFloatingLabels")]
    [Display(Name = "Use Floating Labels")]
    public bool UseFloatingLabels { get; set; }


    public FormElementOptions DeepCopy()
    {
        return new FormElementOptions
        {
            Grid = Grid.DeepCopy(),
            Form = Form.DeepCopy(),
            FormToolbarActions = FormToolbarActions.DeepCopy(),
            GridTableActions = GridTableActions.DeepCopy(),
            GridToolbarActions = GridToolbarActions.DeepCopy(),
            UseFloatingLabels = UseFloatingLabels,
            EnableAuditLog = EnableAuditLog
        };
    }
}