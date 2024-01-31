using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Specifies the behavior of the field.
/// </summary>
public enum FieldBehavior
{
    /// <summary>
    /// Field used only in GET db calls.
    /// </summary>
    Real = 1,

    /// <summary>
    /// Field NOT used in any db calls. Use to customize things at runtime.
    /// </summary>
    Virtual = 2,

    /// <summary>
    /// Field used only in GET db calls.
    /// </summary>
    [Display(Name = "ReadOnly")]
    ViewOnly = 3,
    
    /// <summary>
    /// Field used only in SET db calls.
    /// </summary>
    WriteOnly = 4
}