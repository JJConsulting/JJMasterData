

namespace JJMasterData.Core.DataDictionary.Actions;

/// <summary>
/// When in a relationship, this is the action to edit the parent DataPanel.
/// </summary>
public class FormEditAction : FormToolbarAction
{
    public const string ActionName = "form-edit";

    public FormEditAction()
    {
        Name = ActionName;
        VisibleExpression = "val:1";
        Icon = IconType.Pencil;
        ShowAsButton = true;
        EnableExpression = "val:1";
        Location = FormToolbarActionLocation.Panel;
        Order = 0;
        Text = "Edit";
    }
}