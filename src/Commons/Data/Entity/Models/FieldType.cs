using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Specifies the type of data in the database
/// </summary>
public enum FieldType
{
    Date = 1,
    DateTime = 2,
    Float = 3,
    Int = 4,
    NText = 5,
    NVarchar = 6,
    Text = 7,
    Varchar = 8,
    DateTime2 = 9,
    [Display(Name="Boolean")]
    Bit = 10,
    [Display(Name="Guid")]
    UniqueIdentifier = 11
}