namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridEditAction : GridToolbarAction
{
    public const string ActionName = "grid-edit";
    
    public GridEditAction()
    {
        Name = ActionName;
        Text = "Bulk Edit";
        Icon = IconType.SolidPenToSquare;
        ShowAsButton = true;
        Order = 10;
        VisibleExpression = "val:0";
    }
    
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}