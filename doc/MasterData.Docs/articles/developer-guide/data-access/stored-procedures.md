# Stored procedures and output parameters

Use [`DataAccess`](index.md) from the `JJMasterData.Commons.Data` namespace. The examples assume a configured `dataAccess` instance; see [Basic usage](index.md#basic-usage).

Set the command type to `CommandType.StoredProcedure`. After execution, JJMasterData copies output values back to the corresponding `DataAccessParameter` objects.

```csharp
var newId = new DataAccessParameter(
    "@NewId",
    DBNull.Value,
    DbType.Int32,
    ParameterDirection.Output);

var command = new DataAccessCommand(
    "dbo.CreatePerson",
    [
        new DataAccessParameter("@Name", name),
        newId
    ],
    CommandType.StoredProcedure);

await dataAccess.SetCommandAsync(command, cancellationToken);

var personId = Convert.ToInt32(newId.Value);
```

For variable-length output values, also set the parameter `Size`.

[Data access overview](index.md)
