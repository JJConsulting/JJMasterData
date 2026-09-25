# Localization

JJMasterData uses `IStringLocalizer<MasterDataResources>` for UI labels and messages. Set the request culture with the host application's request-localization middleware.

## Custom localizer

To load strings from application-managed resources, implement `IStringLocalizer<MasterDataResources>` and register it after `AddJJMasterDataWeb`:

```csharp
builder.Services.AddJJMasterDataWeb(builder.Configuration);
builder.Services.AddTransient<IStringLocalizer<MasterDataResources>, ApplicationStringLocalizer>();
```

The default .NET dependency injection container resolves the last registered implementation when a single service is requested, so this registration replaces the localizer used by JJMasterData. See Microsoft's documentation on [service registration and multiple implementations](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection/service-registration#multiple-service-registrations).

HTML templates can access the same localizer with `localize("ResourceKey")`.
