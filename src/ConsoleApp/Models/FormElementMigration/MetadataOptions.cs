#nullable disable
using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;

namespace JJMasterData.ConsoleApp.Models.FormElementMigration;


public class MetadataOptions
{
    [JsonProperty("grid")]
    public GridUI Grid { get; set; } = new();

    [JsonProperty("form")]
    public FormUI Form { get; set; } = new();

    [JsonProperty("toolBarActions")]
    public GridToolbarActions ToolbarActions { get; set; } = new();

    [JsonProperty("gridActions")]
    public GridActions GridActions { get; set; } = new();
}