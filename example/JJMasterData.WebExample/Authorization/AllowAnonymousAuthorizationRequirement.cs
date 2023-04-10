using Microsoft.AspNetCore.Authorization;

namespace JJMasterData.WebExample.Authorization;

public class AllowAnonymousAuthorizationRequirement : AuthorizationHandler<IAuthorizationRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
