using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public enum GridPaginationType
{
    [Display(Name = "Buttons")]
    Buttons,
    [Display(Name = "Scroll")]
    Scroll
}