<h1>Configurations<small> All you can change on JJ MasterData</small></h1>

## How to configure?

There are three ways to configure an application:
<br><br>

**1)** Add configuration key in appsettings.json. On .NET Framework you will need to add `IConfiguration` via `Microsoft.Extensions.Configuration`

<details><summary> >> appsettings.json (click to expand)</summary><br>

> [!TIP] 
> To autocomplete with JJMasterData keys in your text editor, put this URL in the JSON Schema.
https://raw.githubusercontent.com/JJConsulting/JJMasterData.JsonSchema/main/JJMasterData.JsonSchema/jjmasterdata.json

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ConnectionString": "data source=data source=localhost;initial catalog=JJMasterData;Integrated Security=True"
  },
  "JJMasterData": {
    "TableName": "tb_masterdata",
    "ResourcesTableName": "tb_masterdata_resources",
    "PrefixGetProc": "jj_get{tablename}",
    "PrefixSetProc": "jj_set{tablename}",
    "BootstrapVersion":5,
    "Theme": "dark-blue",
    "Logger": {
      "Table": {
        "Name": "tb_masterdata_log"
      },
      "FileName": "yyyyMMdd_applog.txt",
      "WriteInDatabase": "All",
      "WriteInFile": "All"
    },
    "Swagger": {
      "DarkMode": true,
      "Enable": true
    }
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


**2)** Changing code at application startup
```cs
builder.Services.AddJJMasterDataWeb(options =>
{
    options.BootstrapVersion = 5;
    options.ConnectionString = "data source=localhost;initial catalog=JJMasterData;Integrated Security=True";
});
```


**3)** By custom `IConfiguration`
```cs
//Any class that implements a ISettings
builder.Configuration.AddXmlFile("my_custom_source.xml");

//That simple
builder.Services.AddJJMasterDataWeb(builder.Configuration);

//or if you prefer
//builder.Services.AddSingleton(settings);
```

## What to configure?

**Global Options**
```cs
///Edit your options at appsettings.json or pass IConfiguration via parameter.
builder.Services.AddJJMasterDataWeb();
```
You can change any of the [JJMasterDataOptions](https://portal.jjconsulting.tech/jjdoc/lib/JJMasterData.Commons.Options.JJMasterDataOptions.html) as seen above.
<br>

**Log**

```cs
builder.Services.AddLogging(ILogger)
```

You can use any Log tool to implement the interface 
Microsoft.Extensions.Logging.ILogger
<br>
If you need a basic log:

```cs
builder.Services.AddJJMasterDataWeb().WithJJMasterDataLogger();
```

This is a simple application log that can be written to a file, database, etc... 
We use it for testing, because it is not async. To configure the log see [LoggerOptions](https://portal.jjconsulting.tech/jjdoc/lib/JJMasterData.Commons.Logging.LoggerOptions.html)


**Internacionalization**
```cs
builder.Services.AddJJMasterDataWeb();
builder.Services.AddUrlRequestCultureProvider(
    new CultureInfo("zh-CN"),
    new CultureInfo("en-US")
);
```
[read more](internationalization.md) about internationalization.
<br>


