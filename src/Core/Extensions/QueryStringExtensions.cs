using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Extensions;

public static class QueryStringExtensions
{
    public static bool TryGetValue(this IQueryString queryString,string key, out string value)
    {
        value = queryString[key];
        return !string.IsNullOrWhiteSpace(value);
    }   
}