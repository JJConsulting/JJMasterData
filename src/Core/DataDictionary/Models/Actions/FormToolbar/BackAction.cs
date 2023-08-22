#nullable enable


namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public class BackAction : FormToolbarAction
{
    public const string ActionName = "back";

    public BackAction()
    {
        Name = ActionName;
        VisibleExpression = "exp: {pagestate} = 'VIEW'";
        Icon = IconType.ArrowLeft;
        ShowAsButton = true;
        FormToolbarActionLocation = Actions.FormToolbarActionLocation.Panel;
        Order = 0;
        Text = "Back";
    }
}