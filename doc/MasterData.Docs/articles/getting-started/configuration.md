# Installation and configuration

The current `JJMasterData.Web` project targets **.NET 10**. Use an ASP.NET Core MVC or Razor Pages host and SQL Server for this walkthrough; SQL Server is the default entity provider.

```bash
dotnet new mvc -n MasterDataSample -f net10.0
cd MasterDataSample
dotnet add package JJMasterData.Web
```

`JJMasterData.Web` brings the Core and Commons dependencies and the Razor UI assets. Install `JJMasterData.WebApi` only if you need HTTP data endpoints. Keep the JJMasterData packages on the same release.

## Configure the connection

Store the connection under **`JJMasterData:ConnectionString`**. For local development in the MVC project:

```bash
dotnet user-secrets init
dotnet user-secrets set "JJMasterData:ConnectionString" "Server=localhost;Database=MasterDataSample;User Id=sa;Password=<password>;TrustServerCertificate=True"
```

## Configure the host

This minimal `Program.cs` runs the UI locally:

```csharp
using JJMasterData.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddJJMasterDataWeb(builder.Configuration);

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

await app.UseMasterDataSeedingAsync();
app.MapDataDictionary();
app.MapMasterData();
app.Run();
```

`AddJJMasterDataWeb` registers MVC, localization, component factories and data services. `UseMasterDataSeedingAsync` creates the dictionary and audit structures if missing; the database login needs permission to create them. Business tables and stored procedures are created separately through the dictionary editor.

Open `/DataDictionary` to configure metadata. The example has no authentication: add the host's authentication and [route policies](../developer-guide/authorization.md) before exposing the editor or CRUDs to users.

Next: [create the first dictionary](first-dictionary.md).
