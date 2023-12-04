<h1>Configurations<small> All you can change on JJ MasterData</small></h1>

## How to configure?

There are three ways to configure an application:
<br><br>

**1)** Add configuration key in appsettings.json. On .NET Framework you will need to add `IConfiguration` via `Microsoft.Extensions.Configuration`

> [!TIP]
> To autocomplete with JJMasterData keys in your text editor, put this URL in the JSON Schema of your IDE.
(https://raw.githubusercontent.com/JJConsulting/JJMasterData/main/jjmasterdata.json
<details><summary> >> appsettings.json (click to expand)</summary><br>

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ConnectionString": "data source=data source=localhost;initial catalog=JJMasterData;Integrated Security=True"
  },
  "JJMasterData": {
    "DataDictionaryTableName": "tb_masterdata",
    "ResourcesTableName": "tb_masterdata_resources",
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
builder.Services.AddJJMasterDataWeb(options =>
{
    options.ConnectionString = "data source=localhost;initial catalog=JJMasterData;Integrated Security=True";
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

**Global Options**
```cs
///Edit your options at appsettings.json or pass IConfiguration via parameter.
builder.Services.AddJJMasterDataWeb();
```
You can change any property from:

- <xhref:JJMasterData.Commons.Configuration.Options.MasterDataCommonsOptions>
- <xhref:JJMasterData.Core.Configuration.Options.MasterDataCoreOptions>
- <xhref:JJMasterData.Web.Configuration.Options.MasterDataWebOptions>
<br>

**Logging**
[Read more](logging.md) about logging.

**Internationalization**
```cs
builder.Services.AddJJMasterDataWeb();
builder.Services.AddUrlRequestCultureProvider(
    new CultureInfo("zh-CN"),
    new CultureInfo("en-US")
);
```
[Read more](internationalization.md) about internationalization.
<br>


