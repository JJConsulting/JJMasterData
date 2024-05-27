using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace JJMasterData.Commons.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        return value.GetType()
            .GetMember(value.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()
            ?.Name ?? value.ToString();
    }
}