using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridEditAction : GridToolbarAction
{
    public const string ActionName = "grid-edit";
    
    public GridEditAction()
    {
        Name = ActionName;
        Text = "Bulk Edit";
        Icon = FontAwesomeIcon.SolidPenToSquare;
        ShowAsButton = true;
        Order = 10;
        VisibleExpression = "val:0";
    }
    
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}