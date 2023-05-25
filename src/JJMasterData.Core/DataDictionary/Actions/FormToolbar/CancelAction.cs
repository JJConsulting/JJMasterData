#nullable enable


namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public class CancelAction : FormToolbarAction
{
    public const string ActionName = "cancel";
    public override bool IsUserCreated => false;

    public CancelAction()
    {
        Name = ActionName;
        Icon = IconType.Times;
        Order = 0;
        Text = "Cancel";
    }
}