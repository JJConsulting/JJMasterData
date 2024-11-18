using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class InsertAction : GridToolbarAction, IModalAction
{
    public const string ActionName = "insert";
    
    [JsonIgnore]
    public bool ShowOpenedAtGrid => 
        InsertActionLocation is not InsertActionLocation.ButtonAtGrid;

    [Display(Name = "Location")]
    [JsonPropertyName("insertActionLocation")]
    public InsertActionLocation InsertActionLocation { get; set; } = InsertActionLocation.ButtonAtGrid;
    
    /// <summary>
    /// Redirects the insert to another element.
    /// </summary>
    [JsonPropertyName("elementNameToSelect")]
    [Display(Name = "Element Name To Select")]
    public string ElementNameToSelect { get; set; }

    /// <summary>
    /// Re-opens the insert after saving.
    /// </summary>
    [JsonPropertyName("reopenForm")]
    [Display(Name = "Reopen Form")]
    public bool ReopenForm { get; set; }

    [JsonPropertyName("showAsModal")]
    [Display(Name = "Show as Modal")]
    public bool ShowAsModal { get; set; }
    
    [Display(Name = "Modal Title")]
    public string ModalTitle { get; set; }
    
    [Display(Name = "Success Message")]
    public string SuccessMessage { get; set; }

    public InsertAction()
    {
        Name = ActionName;
        Text = "New";
        SuccessMessage = "Record added successfully.";
        Icon = IconType.PlusCircle;
        ShowAsButton = true;
        Order = 1;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}