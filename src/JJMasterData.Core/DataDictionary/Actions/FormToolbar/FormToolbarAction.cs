using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public abstract class FormToolbarAction : BasicAction
{
    public override bool IsUserCreated => false;
}