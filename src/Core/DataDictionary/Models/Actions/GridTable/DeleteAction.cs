using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents the default delete action of a data dictionary
/// </summary>
public sealed class DeleteAction : GridTableAction, ISubmittableAction
{
    [Display(Name = "Is Submit")] 
    public bool IsSubmit { get; set; }

    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "delete";

    public DeleteAction()
    {
        Name = ActionName;
        Tooltip = "Delete";
        ConfirmationMessage = "Would you like to delete this record?";
        Icon = IconType.SolidTrashCan;
        Order = 3;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}