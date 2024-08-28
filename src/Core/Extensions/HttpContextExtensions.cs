using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Extensions;

public static class HttpContextExtensions
{
    public static bool TryGetValue(this IFormValues formValues, string key, out string value)
    {
        value = formValues[key];
        return !string.IsNullOrEmpty(value);
    }   
    public static bool TryGetValue(this IQueryString queryString,string key, out string value)
    {
        value = queryString[key];
        return !string.IsNullOrWhiteSpace(value);
    }   
}