using System.Security.Claims;
using System.Web;
using JJMasterData.Commons.Language;
using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace JJMasterData.Web.Extensions;

public static class HttpContextExtensions
{
    public static string GetUserId(this HttpContext context)
    {
#if DEBUG
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("USERID", "0"));
        context.User.AddIdentity(identity);
#endif
        string? identityId = context.User.Identity?.Name;
        
        if (!string.IsNullOrEmpty(identityId)) return identityId;

        string? userId = context.Session.GetString("USERID");

        if (userId != null) return userId;

        string? claimsId = context.User.Claims.FirstOrDefault(c => c.Type == "USERID")?.Value;
        
        if (claimsId != null) return claimsId;

        throw new HttpException(Translate.Key("Session Expired"));
    }
}