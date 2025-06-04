using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace JJMasterData.Commons.Extensions;

public static class MemberInfoExtensions
{
    public static string GetDisplayName(this MemberInfo member)
    {
        if (member is null)
            throw new ArgumentNullException(nameof(member));

        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();

        if (displayAttribute != null)
        {
            return displayAttribute.Name;
        }

        var displayNameAttribute = member.GetCustomAttribute<DisplayNameAttribute>();

        if (displayNameAttribute != null)
        {
            return displayNameAttribute.DisplayName;
        }

        return member.Name;
    }
}