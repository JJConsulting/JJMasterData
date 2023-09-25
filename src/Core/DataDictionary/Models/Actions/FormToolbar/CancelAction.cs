#nullable enable


namespace JJMasterData.Core.DataDictionary.Actions;

/// <summary>
/// Action to cancel a DataPanel update at a FormView.
/// </summary>
public class CancelAction : FormToolbarAction
{
    public const string ActionName = "cancel";

    public CancelAction()
    {
        Name = ActionName;
        Icon = IconType.Times;
        VisibleExpression = "exp:{pagestate} in ('INSERT','UPDATE')";
        Order = 0;
        Location = FormToolbarActionLocation.Panel;
        ShowAsButton = true;
        Text = "Cancel";
    }
}