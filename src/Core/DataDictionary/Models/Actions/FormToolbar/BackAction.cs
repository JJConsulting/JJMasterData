#nullable enable


namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to return to the Grid at PageState.View.
/// </summary>
public class BackAction : FormToolbarAction
{
    public const string ActionName = "back";

    public BackAction()
    {
        Name = ActionName;
        VisibleExpression = "exp:'{PageState}' = 'View'";
        Icon = IconType.ArrowLeft;
        ShowAsButton = true;
        Location = FormToolbarActionLocation.Bottom;
        Order = 0;
        Text = "Back";
    }
}