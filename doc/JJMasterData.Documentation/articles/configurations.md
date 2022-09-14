<h1>Configurations<small> All you can change on JJ MasterData</small></h1>

<h4>How to configure?</h4>

There are three ways to configure an application:
<br><br>

**1)** Add configuration key in appsettings.json (.NET Core) or web.config (.NET Framework) files

<details><summary> >> appsettings.json</summary><br>

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


<details><summary>>> web.config</summary><br> 

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

<h4>What to configure?</h4>

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
builder.Services.WithTranslator(ITranslator)
```
_WithTranslator_: By default we write the resources in a table, allowed the user create dictionaries dynamically by adding translated words and texts, you can set the table name with the TableResources property in the JJMasterDataSettings class.

<br>

You can change the behavior of system internationalization with the interface [ITranslator](https://portal.jjconsulting.tech/jjdoc/lib/JJMasterData.Commons.Language.ITranslator.html)
<br>

