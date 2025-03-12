# .NET Framework Support

JJMasterData supports legacy .NET Framework systems, including ASP.NET WebForms and MVC5.

## Differences from ASP.NET Core

- JJMasterData.Web is not supported, we recommend starting a [incremental migration](https://devblogs.microsoft.com/dotnet/incremental-asp-net-to-asp-net-core-migration/)
and use the DataDictionary Razor Class Library in a external website. 
- At Global.asax, use AddJJMasterDataCore() instead of AddJJMasterDataWeb()
- You will need to include in your Template.master or _Layout.cshtml, all JJMasterData front-end dependencies, check `_MasterDataScripts` and `_MasterDataStylesheets` source code.
- You will need a custom DI container like SimpleInjector
- After these steps, simply instantiate your <xref:JJMasterData.Core.UI.Components.JJFormView> using <xref:JJMasterData.Core.UI.Components.IComponentFactory> or use the Render route in your external ..NET 8 website.
- You will need to handle your <xref:JJMasterData.Core.UI.Components.ComponentResult> manually