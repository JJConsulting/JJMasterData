#nullable enable

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class PluginAction : BasicAction
{
    public override bool IsUserCreated => true;
}