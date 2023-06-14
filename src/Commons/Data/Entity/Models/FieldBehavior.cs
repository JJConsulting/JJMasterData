namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Specifies the behavior of the field.
/// </summary>
public enum FieldBehavior
{
    /// <summary>
    /// Default behavior of the field. 
    /// Used for Get e Set
    /// </summary>
    Real = 1,

    /// <summary>
    /// Field does not exist in procedures return.
    /// It is not used for Get e Set.
    /// Used to customize a field content at runtime
    /// </summary>
    Virtual = 2,

    /// <summary>
    /// Field using read-only
    /// Only used for Get
    /// </summary>
    ViewOnly = 3
}