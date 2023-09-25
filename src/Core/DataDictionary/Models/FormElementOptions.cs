#nullable enable

using JJMasterData.Core.DataDictionary.Models.Actions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElementOptions
{
    [JsonProperty("gridOptions")]
    public GridUI Grid { get; set; }

    [JsonProperty("formOptions")]
    public FormUI Form { get; set; }

    [JsonProperty("toolBarActions")]
    public GridToolbarActionList GridToolbarActions { get; }

    [JsonProperty("formToolbarActions")]
    public FormToolbarActionList FormToolbarActions { get; }

    [JsonProperty("gridActions")] 
    public GridTableActionList GridTableActions { get; }

    public FormElementOptions()
    {
        Grid = new GridUI();
        Form = new FormUI();
        GridToolbarActions = new GridToolbarActionList();
        FormToolbarActions = new FormToolbarActionList();
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
        GridToolbarActions = gridToolbarActions ?? new GridToolbarActionList();
        GridTableActions = gridTableActions ?? new GridTableActionList();
        FormToolbarActions = formToolbarActions ?? new FormToolbarActionList();
    }
}