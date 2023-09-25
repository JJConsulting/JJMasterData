#nullable enable


namespace JJMasterData.Core.DataDictionary.Actions;

/// <summary>
/// Action to return to the Grid at PageState.View.
/// </summary>
public class BackAction : FormToolbarAction
{
    public const string ActionName = "back";

    public BackAction()
    {
        Name = ActionName;
        VisibleExpression = "exp:{pagestate} = 'VIEW'";
        Icon = IconType.ArrowLeft;
        ShowAsButton = true;
        Location = FormToolbarActionLocation.Bottom;
        Order = 0;
        Text = "Back";
    }
}