<h1>Configurations<small> All you can change on JJ MasterData</small></h1>

## How to configure?

There are three ways to configure an application:
<br><br>

**1)** Add your configuration keys in appsettings.json. On .NET Framework you will need to add `IConfiguration` via `Microsoft.Extensions.Configuration`

> [!TIP]
> To autocomplete with JJMasterData keys in your text editor, put this URL in the JSON Schema of your IDE.
(https://raw.githubusercontent.com/JJConsulting/JJMasterData/main/jjmasterdata.json

<details><summary> >> Example appsettings.json (click to expand)</summary><br>

```json
{
  "AllowedHosts": "*",
  "JJMasterData": {
    "ConnectionString": "data source=data source=localhost;initial catalog=JJMasterData;Integrated Security=True"
    "DataDictionaryTableName": "tb_masterdata",
    "LocalizationTableName": "tb_masterdata_localization",
    "ReadProcedurePattern": "{tablename}Get",
    "WriteProcedurePattern": "{tablename}Set"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
</details>

<br>


**2)** Custom `Action<MasterDataWebOptions>` at application startup
```cs
builder.Services.PostConfigure<MasterDataWebOptions>((webOptions) =>
{
    webOptions.CustomBootstrapPath = "/css/theme.min.css";
    webOptions.LayoutPath = "_MenuLayout";
    webOptions.ModalLayoutPath = "_ModalLayout";
});
```

**3)** By custom `IConfiguration` sources
```cs
//Any IConfiguration sources
builder.Configuration.AddXmlFile("my_custom_source.xml");

//That simple
builder.Services.AddJJMasterDataWeb();
```
## What to configure?

**MasterData Options**
You can change any property from:

- <xref:JJMasterData.Commons.Configuration.Options.MasterDataCommonsOptions>
- <xref:JJMasterData.Core.Configuration.Options.MasterDataCoreOptions>
- <xref:JJMasterData.Web.Configuration.Options.MasterDataWebOptions>
<br>
**Logging**
[Read more](logging.md) about logging.

**Localization**
[Read more](localization.md) about localization.
<br>


