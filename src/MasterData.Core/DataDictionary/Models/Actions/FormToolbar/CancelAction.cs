#nullable enable


using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to cancel a DataPanel update at a FormView.
/// </summary>
public sealed class CancelAction : FormToolbarAction
{
    public const string ActionName = "cancel";
    public override bool IsSystemDefined => true;

    public CancelAction()
    {
        Name = ActionName;
        Icon = FontAwesomeIcon.SolidXmark;
        VisibleExpression = "exp: '{PageState}' = 'Insert' OR '{PageState}' = 'Update'";
        Order = 0;
        Location = FormToolbarActionLocation.Panel;
        ShowAsButton = true;
        Text = "Cancel";
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}