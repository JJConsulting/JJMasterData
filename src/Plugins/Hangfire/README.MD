# Hangfire Plugin

Hangfire support for background tasks.

## How to configure?

```csharp

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection")));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();


builder.Services.AddJJMasterDataWeb().WithHangfire();

///...

app.UseHangfireDashboard();

app.MapDataDictionary();
app.MapMasterData();

await app.UseMasterDataSeedingAsync();
```

## Usage
Simply use exportation and importation like you would use normally.
Check [Hangfire docs](https://docs.hangfire.io/en/latest/getting-started/aspnet-core-applications.html) for more information.

> [!WARNING] 
> User values from session will not work inside Hangfire jobs

