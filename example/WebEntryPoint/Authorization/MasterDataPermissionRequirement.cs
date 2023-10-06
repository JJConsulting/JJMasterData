using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace JJMasterData.WebEntryPoint.Authorization;

public class MasterDataPermissionRequirement : AuthorizationHandler<IAuthorizationRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IAuthorizationRequirement requirement)
    {
        var filterContext = context.Resource as DefaultHttpContext;
        var routeData = filterContext?.HttpContext.GetRouteData();

        if (routeData == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        string? area = null;
        if (routeData.Values.TryGetValue("area", out var areaValue))
            area = areaValue!.ToString();

        if ("MasterData".Equals(area, StringComparison.InvariantCultureIgnoreCase))
        {
            if (routeData.Values.TryGetValue("id", out var elementName))
            {
                if (CanAccessThisElement(elementName as string, context.User))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }
        else if ("DataDictionary".Equals(area, StringComparison.InvariantCultureIgnoreCase))
        {
            //TODO: admin required
        }

        context.Fail();
        return Task.CompletedTask;
    }

    private bool CanAccessThisElement(string? elementName, ClaimsPrincipal user)
    {
        // Code omitted for brevity
        return true;
    }
}