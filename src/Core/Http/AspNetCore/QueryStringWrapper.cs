#if NET
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

public class QueryStringWrapper : IQueryString
{
    private IQueryCollection QueryCollection { get; }
    private QueryString QueryString { get; }
    
    public string this[string key] => QueryCollection[key];

    public string Value => QueryString.Value;

    public QueryStringWrapper(IHttpContextAccessor httpContextAccessor)
    {
        QueryCollection = httpContextAccessor.HttpContext.Request.Query;
        QueryString = httpContextAccessor.HttpContext.Request.QueryString;
    }
}
#endif  