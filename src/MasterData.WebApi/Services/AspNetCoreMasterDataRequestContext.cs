using JJMasterData.Core.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;

namespace JJMasterData.WebApi.Services;

internal sealed class AspNetCoreMasterDataRequestContext(IHttpContextAccessor accessor)
    : IMasterDataRequestContext
{
    private HttpRequest? Request => accessor.HttpContext?.Request;

    public bool HasFormContentType => Request?.HasFormContentType == true;
    public string ApplicationPath => Request?.PathBase.ToString() ?? string.Empty;
    public string ApplicationUri => Request is null
        ? string.Empty
        : new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}").ToString();
    public string AbsoluteUri => Request?.GetDisplayUrl() ?? string.Empty;
    public string? RemoteIpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? UserAgent => Request?.Headers.UserAgent.ToString();
    public string? GetQueryValue(string key) => Request?.Query[key].ToString();
    public string? GetFormValue(string key) => Request?.HasFormContentType == true ? Request.Form[key].ToString() : null;
    public string? GetClaimValue(string claimType) => accessor.HttpContext?.User.FindFirst(claimType)?.Value;
}
