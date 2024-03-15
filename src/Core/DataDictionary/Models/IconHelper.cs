#nullable enable
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Core.DataDictionary.Models;

public static class IconHelper
{
    private static FrozenSet<IconType>? _icons;
    public static FrozenSet<IconType> GetIconList()
    {
        if (_icons != null)
            return _icons;

        _icons = new List<IconType>((IconType[])Enum.GetValues(typeof(IconType))).ToFrozenSet();

        return _icons;
    }
    
    private static FrozenDictionary<IconType, string>? _cssClasses;
    private static FrozenDictionary<IconType, string> CssClasses
    {
        get
        {
            if (_cssClasses is not null) 
                return _cssClasses;
            
            var cssClasses = new Dictionary<IconType, string>();
            var icons = GetIconList();
            foreach (var icon in icons)
            {
                var enumField = typeof(IconType).GetField(icon.ToString());
                var attribute = enumField!.GetCustomAttribute<IconCssClassAttribute>();
                cssClasses[icon] = attribute!.CssClass;
            }

            _cssClasses = cssClasses.ToFrozenDictionary();

            return _cssClasses;
        }
    }
    
    
    public static int GetId(this IconType icon)
    {
        return (int)icon;
    }

    public static string GetCssClass(this IconType icon)
    {
        return CssClasses[icon];
    }
    
    internal static IconType GetIconTypeFromField(ElementField field, object value)
    {
        IconType iconType;
        if (value is int intValue)
        {
            iconType = (IconType)intValue;
        }
        else
        {
            if (int.TryParse(value.ToString(), out var parsedInt))
            {
                iconType = (IconType)parsedInt;
            }
            else
            {
                throw new JJMasterDataException($"Invalid IconType id at {field.LabelOrName}.");
            }
        }

        return iconType;
    }


}