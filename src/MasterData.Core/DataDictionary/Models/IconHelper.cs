#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Core.DataDictionary.Models;

public static class IconHelper
{
    private static readonly FrozenSet<IconType> Icons;
    private static readonly FrozenDictionary<IconType, string> CssClasses;
    
    static IconHelper()
    {
        Icons = new List<IconType>((IconType[])Enum.GetValues(typeof(IconType))).ToFrozenSet();

        var cssClasses = new Dictionary<IconType, string>();
        foreach (var icon in Icons)
        {
            var enumField = typeof(IconType).GetField(icon.ToString());
            var attribute = enumField!.GetCustomAttribute<IconCssClassAttribute>();
            cssClasses[icon] = attribute!.CssClass;
        }
        
        CssClasses = cssClasses.ToFrozenDictionary();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FrozenSet<IconType> GetIconList() => Icons;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetId(this IconType icon) => (int)icon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetCssClass(this IconType icon) => CssClasses[icon];

    internal static IconType GetIconTypeFromField(ElementField field, object value)
    {
        if (value is int intValue)
            return (IconType)intValue;

        return GetIconTypeFromField(field, value.ToString());
    }
    
    public static IconType GetIconTypeFromField(ElementField field, string? value)
    {
        if (int.TryParse(value, out var parsedInt))
            return (IconType)parsedInt;
        throw new JJMasterDataException($"Invalid IconType id at {field.LabelOrName}.");
    }
}