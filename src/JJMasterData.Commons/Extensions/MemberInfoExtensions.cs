using System;
using System.ComponentModel;
using System.Reflection;

namespace JJMasterData.Commons.Extensions
{
    public static class MemberInfoExtensions
    {
        public static string GetDisplayName(this MemberInfo member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            var displayAttribute = member.GetCustomAttribute<DisplayNameAttribute>();

            return displayAttribute is not null ? displayAttribute.DisplayName : member.Name;
        }
    }
}
