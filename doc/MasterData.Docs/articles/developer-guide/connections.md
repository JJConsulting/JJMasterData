# Connections and repositories

`JJMasterData:ConnectionString` configures the default connection. An element with a non-null `ConnectionId` selects an entry from `AdditionalConnectionStrings` by **`Guid`**, not by name.

```json
{
  "JJMasterData": {
    "AdditionalConnectionStrings": [
      {
        "Guid": "28198219-a1b2-4c3d-8e4f-1234567890ab",
        "Name": "Sales",
        "Connection": "<sales database connection string>",
        "ConnectionProvider": "Microsoft.Data.SqlClient"
      }
    ]
  }
}
```

Set the dictionary's `ConnectionId` to that GUID. The actual configuration property names are `Guid` and `Connection`; `Id` and `ConnectionString` are not aliases for these additional-connection properties.

| Extension point | Responsibility |
| --- | --- |
| `IConnectionRepository` | Resolve and list connections; replace with `WithConnectionRepository<T>()`. |
| `IDataDictionaryRepository` | Store metadata; select `WithDatabaseDataDictionary()` or `WithFileSystemDataDictionary(...)`. |
| `IEntityRepository` | Read/write business records using an `Element`. |
| `IEntityProvider` | Provider-specific database operations. |

The builder extensions live in `JJMasterData.Commons.Configuration` and `JJMasterData.Core.Configuration`. Choose metadata storage independently from the record connection.
