#if NET
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

internal class QueryStringWrapper(IHttpContextAccessor httpContextAccessor) : IQueryString
{
    private IQueryCollection QueryCollection { get; } = httpContextAccessor.HttpContext!.Request.Query;
    private QueryString QueryString { get; } = httpContextAccessor.HttpContext.Request.QueryString;

    public string this[string key] => QueryCollection[key];

    public string Value => QueryString.Value;
}
#endif  