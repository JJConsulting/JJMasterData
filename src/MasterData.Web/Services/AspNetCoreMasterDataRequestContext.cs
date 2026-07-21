using JJMasterData.Core.Abstractions;
using JJMasterData.Web.Extensions;

namespace JJMasterData.Web.Services;

internal sealed class AspNetCoreMasterDataRequestContext(IHttpContextAccessor accessor)
    : IMasterDataRequestContext
{
    private HttpRequest? Request => accessor.HttpContext?.Request;

    public bool HasFormContentType => Request?.HasFormContentType == true;
    public string ApplicationPath => Request?.GetApplicationPath() ?? string.Empty;
    public string ApplicationUri => Request?.GetApplicationUri() ?? string.Empty;
    public string AbsoluteUri => Request?.GetAbsoluteUri() ?? string.Empty;
    public string? RemoteIpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? UserAgent => Request?.Headers.UserAgent.ToString();
    public string? GetQueryValue(string key) => Request?.Query[key].ToString();
    public string? GetFormValue(string key) => Request?.GetFormValue(key);
    public string? GetClaimValue(string claimType) => accessor.HttpContext?.User.FindFirst(claimType)?.Value;
}
