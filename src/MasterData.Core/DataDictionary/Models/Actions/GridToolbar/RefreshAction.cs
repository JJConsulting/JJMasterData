using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class RefreshAction : GridToolbarAction
{
    public const string ActionName = "refresh";
    public RefreshAction()
    {
        Name = ActionName;
        Tooltip = "Refresh";
        Icon = FontAwesomeIcon.Refresh;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 6;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}