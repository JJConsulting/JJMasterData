using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public class EditAction : GridTableAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "edit";
    
    [Display(Name = "Show as Modal")]
    public bool ShowAsModal { get; set; }
    public EditAction()
    {
        Name = ActionName;
        Tooltip = "Edit";
        ConfirmationMessage = "";
        Icon = IconType.Pencil;
        Order = 2;
    }


}