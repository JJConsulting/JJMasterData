#nullable enable

namespace JJMasterData.Core.DataDictionary.Actions.FormToolbar;

public class SaveAction : FormToolbarAction
{
    public const string ActionName = "save";

    public SaveAction()
    {
        Order = 1;
        Name = ActionName;
        Icon = IconType.Check;
        Text = "Save";
        FormToolbarActionLocation = Actions.FormToolbarActionLocation.Panel;
        ShowAsButton = true;
        VisibleExpression = "exp:{pagestate} in ('INSERT','UPDATE')";
    }

    public FormEnterKey EnterKeyBehavior { get; set; }
}