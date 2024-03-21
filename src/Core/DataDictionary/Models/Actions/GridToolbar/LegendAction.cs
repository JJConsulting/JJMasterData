namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class LegendAction : GridToolbarAction
{
    public const string ActionName = "legend";
    public LegendAction()
    {
        Name = ActionName;
        Tooltip = "Caption";
        Icon = IconType.Info;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 7;
        SetVisible(false);
    }
}