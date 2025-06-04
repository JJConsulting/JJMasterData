using System;
using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JJMasterData.Commons.Extensions;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Specifies the type of data in the database
/// </summary>
public enum FieldType
{
    [Display(GroupName = "3. DateTime")]
    Date = 1,
    
    [Display(GroupName = "3. DateTime")]
    DateTime = 2,
    
    [Display(GroupName = "2. Numeric")]
    Float = 3,
    
    [Display(GroupName = "2. Numeric")]
    Int = 4,
    
    [Display(GroupName = "1. String")]
    NText = 5,
    
    [Display(GroupName = "1. String")]
    NVarchar = 6,
    
    [Display(GroupName = "1. String")]
    Text = 7,
    
    [Display(GroupName = "1. String")]
    Varchar = 8,
    
    [Display(GroupName = "3. DateTime")]
    DateTime2 = 9,
    
    [Display(Name="Boolean", GroupName="4. Boolean")]
    Bit = 10,
    
    [Display(Name="Guid", GroupName = "5. Especial")]
    UniqueIdentifier = 11,
    
    [Display(GroupName = "3. DateTime")]
    Time = 12,
    
    [Display(GroupName = "2. Numeric")]
    Decimal = 13
}

#if NET
public static class DataTypeHelper
{
    private static readonly FrozenDictionary<FieldType, string> DisplayNameDictionary;

    static DataTypeHelper()
    {
        DisplayNameDictionary = Enum.GetValues<FieldType>()
            .ToDictionary(t => t, t => EnumExtensions.GetDisplayName(t))
            .ToFrozenDictionary();
    }
    
    public static string GetDisplayName(this FieldType type)
    {
        return DisplayNameDictionary[type];
    }
}
#endif