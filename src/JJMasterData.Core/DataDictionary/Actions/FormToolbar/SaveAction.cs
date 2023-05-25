#nullable enable
using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public class SaveAction : BasicAction
{
    public const string ActionName = "save";
    public override bool IsUserCreated => false;

    public SaveAction()
    {
        Name = ActionName;
    }
}