using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class GridToolbarAction : BasicAction
{
    [JsonIgnore]
    public override bool IsUserDefined => false;
}