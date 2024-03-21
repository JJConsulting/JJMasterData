using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class ViewAction : GridTableAction, IModalAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "view";
    
    [Display(Name = "Show as Modal")]
    public bool ShowAsModal { get; set; }
    
    [Display(Name = "Modal Title")]
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
    public override BasicAction DeepCopy() => CopyAction();



}