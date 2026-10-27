# Grid Preference Store

`IGridPreferenceStore` reads and saves one `GridPreferences` object per grid in the store's current scope. It contains display settings, filters, sort order and the current page. Enable
`FormElement.Options.Grid.MaintainValuesOnLoad` ("Save User Preferences" in the dictionary editor) to save and restore
them.

The stores and interface are in `JJMasterData.Core.UI.Components`. Builder methods are in
`JJMasterData.Core.Configuration`.

Consume `IGridPreferenceStore` through constructor injection. The GridView and its factory receive the store directly.

| Store                                 | Storage                   | Lifetime                                                                                                                                                                                      |
|---------------------------------------|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `CookieGridPreferenceStore` | Protected browser cookies | All preferences use browser session cookies without an explicit expiration. |
| `SessionGridPreferenceStore`          | ASP.NET Core session      | All preferences last until the session expires or the host clears them.                                                                                                                       |
| Custom `IGridPreferenceStore`         | Chosen by the host        | Chosen by the implementation.                                                                                                                                                                 |

By default, JJMasterData selects `SessionGridPreferenceStore` when the current request has a session provided by `UseSession()`. Otherwise, it uses `CookieGridPreferenceStore`. The choice is made when the scoped `IGridPreferenceStore` is resolved, so the order of `AddSession()` and `AddJJMasterDataWeb()` does not matter. `AddSession()` alone does not activate the session middleware.

An explicit `WithGridPreferenceStore<T>()`, factory registration or `WithGridSessionPreferenceStore()` overrides automatic selection.

The cookie store isolates preferences by `IMasterDataUser.Id`, application path and grid name. The session store uses the grid name within the current ASP.NET Core session. Both ignore grid-name casing and skip storage when the user ID is missing. `MasterDataCoreOptions.UserIdClaimType` controls the default user ID claim.

## Current session only

Configure the host's session services and middleware. JJMasterData will then use the session store automatically:

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Web.Configuration;

builder.Services.AddDistributedMemoryCache(); // Or use the host's distributed cache.
builder.Services.AddSession();
builder.Services.AddJJMasterDataWeb(builder.Configuration);

var app = builder.Build();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();
```

To require session storage explicitly, use `AddJJMasterDataWeb(...).WithGridSessionPreferenceStore()`. 
This requires `AddSession()` and `UseSession()` and does not fall back to cookies. To force cookies even when a session exists, use `.WithGridPreferenceStore<CookieGridPreferenceStore>()`.

ASP.NET Core session is independent of authentication. Signing out does not automatically clear it. To discard the
current session's data on logout, the host can call `HttpContext.Session.Clear()`; this clears all session entries,
including data unrelated to the grid.

## Custom store

Implement `IGridPreferenceStore`, then register it through the builder:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithGridPreferenceStore<ApplicationGridPreferenceStore>();
```

A factory overload is also available:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithGridPreferenceStore(services => new ApplicationGridPreferenceStore(
        services.GetRequiredService<IHttpContextAccessor>()));
```

Both overloads replace the current store with a scoped registration and return the builder. The last store selection
wins. `WithGridSessionPreferenceStore()` is shorthand for `WithGridPreferenceStore<SessionGridPreferenceStore>()`.

Implement only two methods:

```csharp
public interface IGridPreferenceStore
{
    GridPreferences? Get(string gridName);
    void Set(string gridName, GridPreferences? preferences);
}
```