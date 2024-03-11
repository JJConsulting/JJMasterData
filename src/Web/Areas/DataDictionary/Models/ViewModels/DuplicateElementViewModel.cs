using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class DuplicateElementViewModel
{
    [Display(Name="Original Element Name", Prompt = "Original Element Name")]
    public string? OriginalElementName { get; init; } 
    [Display(Name="New Element Name", Prompt = "New Element Name")]
    public string? NewElementName { get; init; }
}