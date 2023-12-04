
# ConnectionString not found.

This is probably the first error you can find setting up JJMasterData. You can easily 
solve it in different ways.

## .NET 6+
Add a default connection string to appsettings.json.
```json
{
  "ConnectionStrings": {
    "ConnectionString": "data source=localhost,1433;initial catalog=JJMasterData;user id=sa;password=****;"
  }
} 
```
## .NET Framework 4.8
Add a default connection string to web.config.
```xml
<connectionStrings>
		<add name="ConnectionString" 
			 connectionString="data source=localhost;initial catalog=JJMasterData;user id=myuserid;password=****;"
			 providerName="System.Data.SqlClient"/>
</connectionStrings>
```

## .NET Standard 2.0
Using any supported .NET version, you can set programmatically your connection string.
```csharp

var settings = new JJMasterDataSettings
{
    ConnectionString = "YourConnectionString"
}

builder.Services.AddJJMasterDataWeb().WithSettings(settings)
```
