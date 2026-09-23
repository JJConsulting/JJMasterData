# Data access

Use `IDataDictionaryRepository` to load metadata and `IEntityRepository` to query records:

```csharp
var element = await dictionaries.GetFormElementAsync("Person");
var person = await entities.GetFieldsAsync(element,
    new Dictionary<string, object> { ["Id"] = 42 });
```

Here `dictionaries` and `entities` are injected repository instances. Direct entity writes do not run `FormService` validation, form events or audit processing. Use the form service path when those behaviors are required.

For SQL outside a dictionary, inject `DataAccess` and pass parameters:

```csharp
var command = new DataAccessCommand(
    "SELECT Name FROM dbo.Person WHERE Id = @Id",
    new List<DataAccessParameter> { new("@Id", 42) });
var name = await dataAccess.GetResultAsync(command, cancellationToken);
```

`DataAccessCommand` and `DataAccessParameter` are in `JJMasterData.Commons.Data`.
