using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace JJMasterData.Commons.Exceptions
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
