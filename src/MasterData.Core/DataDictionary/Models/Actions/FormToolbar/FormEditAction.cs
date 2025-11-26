

using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// When in a relationship, this is the action to edit the parent DataPanel.
/// </summary>
public sealed class FormEditAction : FormToolbarAction
{
    public const string ActionName = "form-edit";
    public override bool IsSystemDefined => true;
    public FormEditAction()
    {
        Name = ActionName;
        VisibleExpression = "val:0";
        Icon = FontAwesomeIcon.Pencil;
        ShowAsButton = true;
        EnableExpression = "val:1";
        Location = FormToolbarActionLocation.Panel;
        Order = 0;
        Text = "Edit";
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}