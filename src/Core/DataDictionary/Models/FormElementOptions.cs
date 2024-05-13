#nullable enable

using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataDictionary.Models.Actions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementOptions
{
    [JsonProperty("gridOptions")]
    public GridUI Grid { get; set; }

    [JsonProperty("formOptions")]
    public FormUI Form { get; set; }

    [JsonProperty("toolBarActions")]
    public GridToolbarActionList GridToolbarActions { get; private init;  }

    [JsonProperty("formToolbarActions")]
    public FormToolbarActionList FormToolbarActions { get; private init; }

    [JsonProperty("gridActions")] 
    public GridTableActionList GridTableActions { get; private init;  }
    
    [JsonProperty("enableAuditLog")]
    [Display(Name = "Enable Audit Log")]
    public bool EnableAuditLog { get; set; }
    
    [JsonProperty("useFloatingLabels")]
    [Display(Name = "Use Floating Labels")]
    public bool UseFloatingLabels { get; set; }

    public FormElementOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        GridToolbarActions = [];
        FormToolbarActions = [];
        GridTableActions = new GridTableActionList();
    }

    [JsonConstructor]
    private FormElementOptions(
        [JsonProperty("gridOptions")]GridUI? gridUI,
        [JsonProperty("formOptions")]FormUI? formUI,
        [JsonProperty("gridActions")] GridTableActionList? gridTableActions,
        [JsonProperty("toolbarActions")] GridToolbarActionList? gridToolbarActions,
        FormToolbarActionList? formToolbarActions)
    {
        Grid = gridUI ?? new GridUI();
        Form = formUI ?? new FormUI();
        GridToolbarActions = gridToolbarActions ?? [];
        GridTableActions = gridTableActions ?? [];
        FormToolbarActions = formToolbarActions ?? [];
    }

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