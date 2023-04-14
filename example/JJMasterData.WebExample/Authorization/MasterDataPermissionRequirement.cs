using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace JJMasterData.WebExample.Authorization;

public class MasterDataPermissionRequirement : AuthorizationHandler<IAuthorizationRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        var filterContext = context.Resource as DefaultHttpContext;
        var routeData = filterContext?.HttpContext.GetRouteData();

        if (routeData == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        string? area = null;
        if (routeData.Values.ContainsKey("area"))
            area = routeData.Values["area"]!.ToString();

        string? dictionaryName = null;
        if (routeData.Values.ContainsKey("id"))
            dictionaryName = routeData.Values["id"]!.ToString();

        if ("MasterData".ToLower().Equals(area?.ToLower()))
        {
            if (HasDictionaryAccess(dictionaryName, context.User))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        if ("DataDictionary".ToLower().Equals(area?.ToLower()))
        {
            //TODO: admin required
        }

        if ("Log".ToLower().Equals(area?.ToLower()))
        {
            //TODO: access to log system
        }

        context.Fail();
        return Task.CompletedTask;
    }

    private bool HasDictionaryAccess(string? dictionaryName, ClaimsPrincipal user)
    {
        // Code omitted for brevity
        return true;
    }

}