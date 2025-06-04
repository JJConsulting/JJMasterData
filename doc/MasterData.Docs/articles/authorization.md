<h1>
    Authorization
    <small> How to secure your routes on JJ MasterData</small>
</h1>

JJMasterData uses 2 areas on your routes:

- **DataDictionary** It's used to manage forms that only the admin should access;
- **MasterData** Is used to render a form, you must check if user has access;

> [!IMPORTANT]
> You must create your own policies for *DataDictionary* and *MasterData* areas.

> [!IMPORTANT]
> If a user has the claim DataDictionary, we will enable a button to edit the FormElement at /Render route.

Keep in mind that the end-user only access the MasterData Area.<br>

You can also inject your custom authorization requirements for routes using:

```cs
app.UseAuthentication();
app.UseAuthorization();

app.MapDataDictionary()
    .RequireAuthorization(builder=>builder.RequireClaim("DataDictionary"));
app.MapMasterData()
    .RequireAuthorization("MasterDataPolicy");
    
await app.UseMasterDataSeedingAsync();
```

If you are not familiarized with the concept of policies, please check
this [link](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0).

You will need in your application, implement an **authentication**  service, before implementing an **authorization** one.
Please
check [ASP.NET Core docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0)
for more information.
<br>

You can use any supported authentication service, like Microsoft Identity.

To apply your authorization rules on these policies, use the example bellow in your .NET application Program.cs.

```cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MasterDataPolicy", policy =>
    {
        policy.AddRequirements(new MasterDataPermissionRequirement());
    });
});
```

In the example below, we create an `MasterDataPermissionRequirement` to see if a user has access to a specific element:

```csharp
public sealed class MasterDataPermissionRequirement : AuthorizationHandler<IAuthorizationRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAuthorizationRequirement requirement)
    {
        var filterContext = (DefaultHttpContext)context.Resource!;
        var routeData = filterContext.HttpContext.GetRouteData();

        string? area = null;
        if (routeData.Values.TryGetValue("area", out var areaValue))
            area = areaValue!.ToString()?.ToLowerInvariant();
        
        string? elementName = null;
        if (routeData.Values.TryGetValue("elementName", out var elementNameValue))
            elementName = elementNameValue!.ToString();
        
        if ("MasterData".Equals(area, StringComparison.InvariantCultureIgnoreCase))
        {
            if (CanAccessElement(elementName, context.User))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
        else if ("DataDictionary".Equals(area, StringComparison.InvariantCultureIgnoreCase))
        {
            //TODO: admin required
        }

        context.Succeed(this);
        return Task.CompletedTask;
    }
    private static bool CanAccessElement(string? elementName, ClaimsPrincipal user)
    {
        // Code omitted for brevity
        return true;
    }
}
```

If you want to protect specific actions or fields in your DataDictionary, you will need to implement your
own [JJFormView](components/form_view.md) in your custom view.
<br>

> [!TIP]
> Don't forget to secure every policy in **production**, this is just a example for learning purposes.

