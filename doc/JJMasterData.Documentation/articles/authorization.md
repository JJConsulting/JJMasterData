<h1>
    Authorization
    <small> How to secure your routes on JJ MasterData</small>
</h1>

JJMasterData uses 3 areas on your routes: 

- **DataDictionary** It's used to manage forms that only the admin should access;
- **MasterData** Is used to render a form, you must check if user has access;
- **Tools** It's used to configure JJMasterData dependencies, like i18n and logging;

Keep in mind that the end-user only access the MasterData Area.<br>

You can also inject your custom attributes or policy for routes using:
```cs
app.UseAuthentication();
app.UseAuthorization();
app.UseJJMasterDataWeb();
app.MapJJMasterData()
    .RequireAuthorization("MasterDataPolicy");
```
If you are not familiarized with the concept of policies, please check this [link](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0).

You will need in your application, implement a **authentication**  service, before implementing a **authorization** one.
Please check [ASP.NET Core docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) for more information.
<br>
In the example bellow, I'm using a basic cookie authentication to simplify the process.
<br>
You can use any supported authentication service.

```cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SharedCookie";
}).AddCookie("SharedCookie");
```

To apply your authorization rules on these policies, use the example bellow in your .NET 6 application Program.cs.

```cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MasterDataPolicy", policy =>
    {
        policy.AddRequirements(new MasterDataPermissionRequirement());
    });
});
```

MasterDataPermissionRequirement class example:
```csharp
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

        if ("MasterData".Equals(area, StringComparison.InvariantCultureIgnoreCase))
        {
            if (HasDictionaryAccess(dictionaryName, context.User))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
        else if ("DataDictionary".Equals(area, StringComparison.InvariantCultureIgnoreCase))
        {
            //TODO: admin required
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
```

If you want to protect specific actions or fields in your DataDictionary, you will need to implement your own [JJFormView](components/form_view.md) in your custom View or use the <xref:JJMasterData.Core.FormEvents.Abstractions.IFormEvent> interface in the method OnInstanceCreated, customizing your JJFormView object. 
<br>
[Learn more here.](custom_rules.md)

> [!TIP] 
> Don't forget to secure every policy in **production**, this is just a example for learning purposes.

