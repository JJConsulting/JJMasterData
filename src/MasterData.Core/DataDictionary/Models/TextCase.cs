using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public enum TextCase
{
    [Display(Name = "None")]
    None,
    [Display(Name = "UPPERCASE")]
    Uppercase,
    [Display(Name = "lowercase")]
    Lowercase,
    [Display(Name = "TitleCase")]
    TitleCase
}
