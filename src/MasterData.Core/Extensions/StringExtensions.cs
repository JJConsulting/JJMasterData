#if !NET
using System;

namespace JJMasterData.Core.Extensions;

public static class StringExtensions
{
    public static bool Contains(this string str, string substring, StringComparison comparisonType)
    {
        return str.IndexOf(substring, comparisonType) >= 0;                      
    }
}
#endif