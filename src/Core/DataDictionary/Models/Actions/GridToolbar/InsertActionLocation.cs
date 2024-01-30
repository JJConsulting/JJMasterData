using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public enum InsertActionLocation
{
    [Display(Name = "Button At Grid")]
    ButtonAtGrid,
    [Display(Name = "Above Grid")]
    AboveGrid,
    [Display(Name = "Below Grid")]
    BelowGrid
}