using System;
using System.ComponentModel;
using System.Linq;

namespace JJMasterData.Commons.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes &&
            attributes.Any())
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}