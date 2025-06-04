using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// First option at a ComboBox
/// </summary>
public enum FirstOptionMode
{
    /// <summary>
    /// No first option.
    /// </summary>
    [Display(Name="None")]
    None = 1,

    /// <summary>f
    /// Show (All) at the first option
    /// </summary>
    [Display(Name="All")]
    All = 2,

    /// <summary>
    /// Show (Choose) at the first option
    /// </summary>
    [Display(Name="Choose")]
    Choose = 3
}