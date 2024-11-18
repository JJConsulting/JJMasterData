using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class ViewAction : GridTableAction, IModalAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "view";
    
    [Display(Name = "Show as Modal")]
    [JsonPropertyName("showAsModal")]
    public bool ShowAsModal { get; set; }
    
    [Display(Name = "Modal Title")]
    [JsonPropertyName("modalTitle")]
    public string ModalTitle { get; set; }
    
    public ViewAction()
    {
        Name = ActionName;
        Tooltip = "View";
        ConfirmationMessage = "";
        IsDefaultOption = true;
        Icon = IconType.Eye;
        Order = 1;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();



}