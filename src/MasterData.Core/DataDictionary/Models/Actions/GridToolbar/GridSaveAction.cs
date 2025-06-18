namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridSaveAction : GridToolbarAction
{
    public override bool IsSystemDefined => true;
    
    public GridSaveAction()
    {
        Order = 1;
        Name = "grid-save";
        VisibleExpression = "val:0";
        Icon = IconType.Check;
        Text = "Save";
        Location = FormToolbarActionLocation.Panel;
        Color = BootstrapColor.Primary;
        ShowAsButton = true;
    }
    
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}