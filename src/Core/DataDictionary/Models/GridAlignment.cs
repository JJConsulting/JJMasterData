using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public enum GridAlignment
{
    [Display(Name = "Default")]
    Default,
    [Display(Name = "Left")]
    Left,
    [Display(Name = "Center")]
    Center,
    [Display(Name = "Right")]
    Right
}