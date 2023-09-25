#nullable enable


namespace JJMasterData.Core.DataDictionary.Actions;

public class CancelAction : FormToolbarAction
{
    public const string ActionName = "cancel";

    public CancelAction()
    {
        Name = ActionName;
        Icon = IconType.Times;
        VisibleExpression = "val:1";
        Order = 0;
        Location = FormToolbarActionLocation.Panel;
        ShowAsButton = true;
        Text = "Cancel";
    }
}