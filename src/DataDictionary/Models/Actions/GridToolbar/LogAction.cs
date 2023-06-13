namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class LogAction : GridToolbarAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "log";
    public LogAction()
    {
        Name = ActionName;
        ToolTip = "Log";
        Icon = IconType.Film;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 20;
        SetVisible(false);
    }
}