<h1>Configurations<small> All you can change on JJ MasterData</small></h1>

## How to configure?

There are three ways to configure an application:
<br><br>

**1)** Add configuration key in appsettings.json (.NET Core) or web.config (.NET Framework) files

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

<details><summary>>> web.config (click to expand)</summary><br> 

```xml
<?xml version="1.0"?>
<configuration>
	<appSettings>
		<!--System Log (Error, Warning, Information, All, None) Default Value (None)-->
		<add key="log_writeinconsole" value="None"/>
		<add key="log_writeineventviewer" value="None"/>
		<add key="log_writeintrace" value="None"/>
		<add key="log_writeinfile" value="All"/>
		<add key="log_filename" value="App_Data\log\yyyyMMdd_applog.txt"/>
		<add key="log_writeindatabase" value="All"/>
		<add key="log_tablename" value="tb_masterdata_log"/>
		<add key="log_connectname" value="ConnectionString"/>
		<add key="app.connectionstring" value="ConnectionString"/>
		
		<!--JJMasterData Settings-->
		<add key="JJMasterData.PrefixProcGet" value="jj_get{tablename}"/>
		<add key="JJMasterData.PrefixProcSet" value="jj_set{tablename}"/>
		<add key="JJMasterData.TableName" value="tb_masterdata"/>
		<add key="JJMasterData.TableResources" value="tb_masterdata_resources"/>
		<add key="JJMasterData.URL" value="https://localhost/masterdata/"/>
		<add key="JJMasterData.BootstrapVersion" value="3"/>
		<add key="JJMasterData.BootstrapTheme" value="dark-blue"/>
		
		<!--Layout Settings-->
		<add key="JJMasterData.LayoutUrl" value="~/Views/Shared/_Layout.vbhtml"/>
		<add key="JJMasterData.LayoutUrlPopup" value="~/Views/Shared/_Layout.Popup.vbhtml"/>

	</appSettings>
	<connectionStrings>
		<add name="Connectionstring" connectionString="data source=data source=localhost;initial catalog=JJMasterData;Integrated Security=True" providerName="System.Data.SqlClient"/>
	</connectionStrings>
	
</configuration>
```
</details>
<br>


**2)** Changing code at application startup
```cs
builder.Services.AddJJMasterDataWeb().WithSettings(options =>
{
    options.BootstrapVersion = 5;
    options.ConnectionString = "data source=localhost;initial catalog=JJMasterData;Integrated Security=True";
});
```


**3)** By dependency injection implementing the object JJMasterData.Commons.Settings.ISettings
```cs
//Any class that implements a ISettings
var settings = new JJMasterDataSettings();

//That simple
builder.Services.AddJJMasterDataWeb().WithSettings(settings);

//or if you prefer
//builder.Services.AddSingleton(settings);
```

## What to configure?

**Global Settings**
```cs
builder.Services.AddJJMasterDataWeb().WithSettings();
```
_WithSettings_: Any property of classes [JJMasterDataSettings](https://portal.jjconsulting.tech/jjdoc/lib/JJMasterData.Commons.Settings.JJMasterDataSettings.html) as seen above.
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
We use it for testing, because it is not async. To configure the log see [LoggerSettings](https://portal.jjconsulting.tech/jjdoc/lib/JJMasterData.Commons.Logging.LoggerSettings.html)


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


