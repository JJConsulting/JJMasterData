namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class UserCreatedAction : BasicAction
{
    public override bool IsUserCreated => true;
}