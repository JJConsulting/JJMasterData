
# ConnectionString not found.

This is probably the first error you can find setting up JJMasterData. You can easily 
solve it in different ways.

## ..NET 8+
Add a default connection string to appsettings.json.
```json
{
  "JJMMasterData": {
    "ConnectionString": "data source=localhost,1433;initial catalog=JJMasterData;user id=sa;password=****;"
  }
} 
```

## .NET Framework 4.8 and .NET Standard 2.0
Using any supported .NET version, you can set programmatically your connection string.
```csharp
builder.Services.AddJJMasterDataCore(new MasterDataCoreOptionsConfiguration()
{
    ConfigureCommons = commons =>
    {
        commons.ConnectionString = "Read From Web.Config or Whatever Place You Want";
    } 
})
```
