using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridCancelAction : GridToolbarAction
{
    public override bool IsSystemDefined => true;
    public GridCancelAction()
    {
        Name = "grid-cancel";
        Icon = FontAwesomeIcon.SolidXmark;
        Order = 0;
        Location = FormToolbarActionLocation.Panel;
        ShowAsButton = true;
        Text = "Cancel";
        VisibleExpression = "val:0";
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}