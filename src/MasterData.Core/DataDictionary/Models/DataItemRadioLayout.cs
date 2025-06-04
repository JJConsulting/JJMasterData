using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

public enum DataItemRadioLayout
{
    [Display(Name="Horizontal")]
    Horizontal,
    [Display(Name="Vertical")]
    Vertical,
    [Display(Name="Buttons")]
    Buttons
}