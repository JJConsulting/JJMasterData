namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class LegendAction : GridToolbarAction
{
    public const string ActionName = "legend";
    public LegendAction()
    {
        Name = ActionName;
        ToolTip = "Information";
        Icon = IconType.Info;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 7;
        SetVisible(false);
    }
}