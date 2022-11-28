using Microsoft.AspNetCore.Http;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace JJMasterData.Web.Extensions;

public static class HttpContextExtensions
{
    public static string? GetUserId(this HttpContext context)
    {
        string? userId = context.Session.GetString("USERID");
        if (userId != null) 
            return userId;

        userId = context.User.Claims.FirstOrDefault(c => c.Type == "USERID")?.Value;

        return userId;
    }
}