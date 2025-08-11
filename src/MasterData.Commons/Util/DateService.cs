using System;
using JetBrains.Annotations;

namespace JJMasterData.Commons.Util;

[Obsolete("Please use RelativeDateFormatter instead.", error:false)]
[PublicAPI]
public class DateService(RelativeDateFormatter formatter)
{
    public string GetPhrase(DateTime date)
    {
        return formatter.ToRelativeString(date);
    }
}