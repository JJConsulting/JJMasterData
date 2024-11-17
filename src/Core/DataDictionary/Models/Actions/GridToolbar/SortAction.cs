namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class SortAction : GridToolbarAction
{
    public const string ActionName = "sort";
    public override bool IsCustomAction => false;
    public SortAction()
    {
        Name = ActionName;
        Tooltip = "Sort";
        Icon = IconType.SortAlphaAsc;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 7;
        SetVisible(false);
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}