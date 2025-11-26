using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class SortAction : GridToolbarAction
{
    public const string ActionName = "sort";
    
    [JsonIgnore]
    public override bool IsUserDefined => false;
    public SortAction()
    {
        Name = ActionName;
        Tooltip = "Sort";
        Icon = FontAwesomeIcon.SortAlphaAsc;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 7;
        SetVisible(false);
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}