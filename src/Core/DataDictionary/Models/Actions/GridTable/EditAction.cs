using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class EditAction : GridTableAction, IModalAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "edit";
    
    [Display(Name = "Show as Modal")]
    public bool ShowAsModal { get; set; }
    
    [Display(Name = "Modal Title")]
    public string ModalTitle { get; set; }

    public EditAction()
    {
        Name = ActionName;
        Tooltip = "Edit";
        ConfirmationMessage = "";
        Icon = IconType.Pencil;
        Order = 2;
    }


}