<h1>Configurations<small> All you can change on JJ MasterData</small></h1>

<h4>Como configurar?</h4>
Existem tres maneiras de configurar a aplicação:
<br><br>

**1)** Incluindo as chaves de configuração nos arquivos appsettings.json (.NET Core) ou web.config (.NET Framework)
<details><summary>appsettings.json</summary><br>

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ConnectionString": "data source=..."
  },
  "JJMasterData": {
    "TableName": "tb_masterdata",
    "ResourcesTableName": "tb_masterdata_resources",
    "PrefixGetProc": "jj_get{tablename}",
    "PrefixSetProc": "jj_set{tablename}",
    "UseUrlRequestCultureProvider": true,
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


<details><summary>web.config</summary><br> 

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
		
		<!--Configurações JJMasterData-->
		<add key="JJMasterData.PrefixProcGet" value="jj_get{tablename}"/>
		<add key="JJMasterData.PrefixProcSet" value="jj_set{tablename}"/>
		<add key="JJMasterData.TableName" value="tb_masterdata"/>
		<add key="JJMasterData.TableResources" value="tb_masterdata_resources"/>
		<add key="JJMasterData.URL" value="https://localhost/masterdata/"/>
		<add key="JJMasterData.BootstrapVersion" value="3"/>
		<add key="JJMasterData.BootstrapTheme" value="dark-blue"/>
		
		<!--Configurações Layout-->
		<add key="JJMasterData.LayoutUrl" value="~/Views/Shared/_Layout.vbhtml"/>
		<add key="JJMasterData.LayoutUrlPopup" value="~/Views/Shared/_Layout.Popup.vbhtml"/>

	</appSettings>
	<connectionStrings>
		<add name="Connectionstring" connectionString="data source=..." providerName="System.Data.SqlClient"/>
	</connectionStrings>
	
</configuration>
```


</details>

**2)** Alterando programaticamente no startup da aplicação
```cs
builder.Services.AddJJMasterDataWeb().WithSettings(options =>
{
    options.BootstrapVersion = 5;
    options.ConnectionString = "...";
});
```


**3)** Por injeção de dependecia implementando o objeto JJMasterData.Commons.Settings.ISettings
```cs
//Qualquer classe que implemente a ISettings
var settings = new JJMasterDataSettings();

//Simples asssim
builder.Services.AddJJMasterDataWeb().WithSettings(settings);

//ou se preferir
//builder.Services.AddSingleton(settings);
````

<h4>O que configurar?</h4>

**WithSettings:** Qualquer propriedade das classes [JJMasterDataSettings](../lib/JJMasterData.Commons.Settings.JJMasterDataSettings.html) como visto acima.
<br>

**WithJJMasterDataLogger:**  Esse é simples log da aplicação que pode ser guardado em arquivo, banco de dados, etc... 
Nós utilizamos para testes pois ele não é async, porém você poderá utilizar qualquer ferramenta de Log implementando a interface 
Microsoft.Extensions.Logging.ILogger exemplo:
```cs
builder.Services.AddLogging(...)
```

**WithTranslator:** Por padrão gravamos os resources em uma tabela, pois o usuário pode criar dicionários dinamicamente adicionando palavras e textos traduzidos, você pode configurar o nome da tabela com a propriedade TableResources na classe JJMasterDataSettings.
<br>
Você pode alterar o comportamento da internacionalização do sistema com a interface [ITranslator](../lib/JJMasterData.Commons.Language.ITranslator.html)
<br>


**WithBackgroundTask:** 

**WithDataAccess:** 

Configurar como os serviços de exportação e importação rodarão em segundo cm

ou LoggerSettings como
