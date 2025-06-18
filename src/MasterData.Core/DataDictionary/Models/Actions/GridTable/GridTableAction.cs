using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class GridTableAction : BasicAction
{
    [JsonIgnore]
    public override bool IsUserDefined => false;
}