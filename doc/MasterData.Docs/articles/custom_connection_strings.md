# Custom Connection Strings

## IOptions / appsettings.json

It's possible to add custom connection strings to the `appsettings.json` file. This is useful when you want to connect to different databases.

```json
{
  "JJMasterData": {
    "AdditionalConnectionStrings": [
        {
            "Id": "28198219-a1b2-4c3d-8e4f-1234567890ab",
            "Name": "MyCustomConnectionString",
            "Connection": "data source=localhost;initial catalog=MyCustomDatabase;Integrated Security=True"
        },
        {
            "Id": "2813dd9-a1b2-4cs1-8e4f-1234567890ab",
            "Name": "AnotherConnectionString",
            "ConnectionString": "data source=localhost;initial catalog=AnotherDatabase;Integrated Security=True",
            "ConnectionProvider": "Microsoft.Data.SqlClient"
        }
    ]
  }
}
```

If you do not specify the `ConnectionProvider`, it will default to `Microsoft.Data.SqlClient`.

## Custom IConnectionRepository implementation

You can also create a custom `IConnectionRepository` implementation to manage your connection strings. This is useful when you want to have more control over how the connection strings are managed and used.

```csharp
builder.Services.AddJJMasterDataWeb().WithConnectionRepository<MyCustomConnectionRepository>();
``` 