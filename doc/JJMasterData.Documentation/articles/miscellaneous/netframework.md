# .NET Framework Support

JJMasterData supports legacy .NET Framework systems, including ASP.NET WebForms and MVC5.

## Differences from .NET 6

- JJMasterData.Web is not supported, we recommend starting a [incremental migration](https://devblogs.microsoft.com/dotnet/incremental-asp-net-to-asp-net-core-migration/)
and use the DataDictionary Razor Class Library in a external website. 
- At Global.asax, use AddJJMasterDataCore() and UseJJMasterData()
- You will need to include in your Template.master or _Layout.cshtml, all JJMasterData front-end dependencies, check _MasterDataScripts and _MasterDataStylesheets source code.
- After these steps, simply instantiate your JJFormView or use /Render in your external .NET 6 website.
