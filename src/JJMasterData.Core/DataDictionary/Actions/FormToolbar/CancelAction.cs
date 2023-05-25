#nullable enable
using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public class CancelAction : BasicAction
{
    public const string ActionName = "cancel";
    public override bool IsUserCreated => false;

    public CancelAction()
    {
        Name = ActionName;
    }
}