

namespace JJMasterData.Core.DataDictionary.Actions;

public class FormEditAction : FormToolbarAction
{
    public const string ActionName = "form-edit";

    public FormEditAction()
    {
        Name = ActionName;
        VisibleExpression = "val:1";
        Icon = IconType.Pencil;
        ShowAsButton = true;
        FormToolbarActionLocation = DataDictionary.Actions.FormToolbarActionLocation.Panel;
        Order = 0;
        Text = "Edit";
    }
}