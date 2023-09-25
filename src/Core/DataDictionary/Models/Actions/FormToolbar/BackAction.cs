#nullable enable


namespace JJMasterData.Core.DataDictionary.Actions;

public class BackAction : FormToolbarAction
{
    public const string ActionName = "back";

    public BackAction()
    {
        Name = ActionName;
        VisibleExpression = "val:1";
        Icon = IconType.ArrowLeft;
        ShowAsButton = true;
        Location = FormToolbarActionLocation.Panel;
        Order = 0;
        Text = "Back";
    }
}