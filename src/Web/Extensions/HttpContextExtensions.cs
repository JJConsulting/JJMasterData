using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace JJMasterData.Web.Extensions;

public static class HttpContextExtensions
{
    public static string? GetUserId(this HttpContext context)
    {
        return context.User.Identity?.Name;
    }
}