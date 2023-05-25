#nullable enable

namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public class SaveAction : FormToolbarAction
{
    public const string ActionName = "save";
    public override bool IsUserCreated => false;

    public SaveAction()
    {
        Order = 1;
        Name = ActionName;
        Icon = IconType.Check;
        Text = "Save";
    }
}