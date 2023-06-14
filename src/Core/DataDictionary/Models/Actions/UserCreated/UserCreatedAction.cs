using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;

public abstract class UserCreatedAction : BasicAction
{
    public override bool IsUserCreated => true;
}