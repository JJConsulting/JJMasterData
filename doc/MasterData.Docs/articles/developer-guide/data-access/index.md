# Data access

`DataAccess` is JJMasterData's lightweight ADO.NET wrapper for executing SQL that is not represented by a data dictionary. It uses the database provider and connection string configured for JJMasterData, opens and disposes connections for each operation, and supports synchronous and asynchronous APIs.

Use `DataAccess` for custom queries, commands, and stored procedures. For CRUD operations based on an `Element`, prefer [`IEntityRepository`](~/articles/developer-guide/repositories/entities.md), which applies the element metadata and resolves its configured connection.

The types in this guide are in the `JJMasterData.Commons.Data` namespace.

## Basic usage

Register JJMasterData in the host and inject `DataAccess` into the service that needs it. `AddJJMasterDataCommons`, `AddJJMasterDataCore`, and `AddJJMasterDataWeb` register `DataAccess` as a scoped service.

```csharp
using JJMasterData.Commons.Data;

public sealed class PersonReportService(DataAccess dataAccess)
{
    public async Task<string?> GetNameAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var command = new DataAccessCommand(
            "SELECT Name FROM dbo.Person WHERE Id = @Id",
            [new DataAccessParameter("@Id", personId)]);

        return await dataAccess.GetStringAsync(command, cancellationToken);
    }
}
```

The injected instance uses `JJMasterData:ConnectionString` and `JJMasterData:ConnectionProvider`. See [connections and repositories](~/articles/developer-guide/repositories/index.md) for the complete connection configuration.

You can also create an instance directly when a query must use a connection that is not the JJMasterData default:

```csharp
var dataAccess = new DataAccess(
    connectionString,
    DataAccessProvider.PostgreSql);
```

## Query data

Select the method that matches the expected result:

| Expected result | Asynchronous method | Return value |
| --- | --- | --- |
| First column of the first row | `GetResultAsync` | `object?` |
| Rows and columns | `GetDataTableAsync` | `DataTable` |
| Multiple result tables | `GetDataSetAsync` | `DataSet` |
| First row keyed by column name | `GetDictionaryAsync` | `Dictionary<string, object?>` |
| All rows keyed by column name | `GetDictionaryListAsync` | `List<Dictionary<string, object?>>` |
| `INSERT`, `UPDATE`, or `DELETE` | `SetCommandAsync` | Number of affected rows |
| Several writes in one transaction | `SetCommandListAsync` | Total number of affected rows |

The methods in the table have synchronous equivalents without the `Async` suffix. In web applications, prefer the asynchronous methods and pass the request's `CancellationToken`.

### Read one value

These methods read the first column of the first row. They accept a `DataAccessCommand` and an optional `CancellationToken`, and return `null` for an empty result or SQL `NULL`.

| Method | Return type |
| --- | --- |
| `GetStringAsync` | `Task<string?>` |
| `GetIntAsync` | `Task<int?>` |
| `GetLongAsync` | `Task<long?>` |
| `GetDecimalAsync` | `Task<decimal?>` |
| `GetBoolAsync` | `Task<bool>` |
| `GetGuidAsync` | `Task<Guid?>` |
| `GetDateTimeAsync` | `Task<DateTime?>` |

```csharp
var command = new DataAccessCommand(
    "SELECT COUNT(*) FROM dbo.Person WHERE IsActive = @IsActive",
    [new DataAccessParameter("@IsActive", true)]);

int? activePeople = await dataAccess.GetIntAsync(command, cancellationToken);
```

They use typed getters when the provider type matches, with validated conversions otherwise. Invalid conversions and numeric overflow throw `DataException`. These methods have no synchronous or raw SQL string overloads.

Use `GetResultAsync` when you need an `object?`. It returns `null` for an empty result; SQL `NULL` may be returned as `DBNull.Value`.

### Read rows

Use `GetDictionaryListAsync` when dynamic, column-name-based access is convenient:

```csharp
var command = new DataAccessCommand(
    """
    SELECT Id, Name, Email
    FROM dbo.Person
    WHERE IsActive = @IsActive
    ORDER BY Name
    """,
    [new DataAccessParameter("@IsActive", true)]);

var people = await dataAccess.GetDictionaryListAsync(command, cancellationToken);

foreach (var person in people)
{
    var id = Convert.ToInt32(person["Id"]);
    var name = Convert.ToString(person["Name"]);
}
```

Dictionary keys are case-insensitive. `GetDictionaryListAsync` normalizes database `NULL` values to C# `null`.

Use `GetDataTableAsync` instead when the consumer expects ADO.NET objects, such as a reporting or export API:

```csharp
DataTable table = await dataAccess.GetDataTableAsync(command, cancellationToken);
```

## Commands and transactions

`SetCommandAsync` executes a non-query command and returns its affected-row count:

```csharp
var command = new DataAccessCommand(
    """
    UPDATE dbo.Person
    SET Email = @Email
    WHERE Id = @Id
    """,
    [
        new DataAccessParameter("@Email", email),
        new DataAccessParameter("@Id", personId)
    ]);

var affectedRows = await dataAccess.SetCommandAsync(command, cancellationToken);
```

To execute several commands atomically, pass them to `SetCommandListAsync`. The method opens one transaction, commits after every command succeeds, and rolls back if any command fails.

```csharp
var commands = new[]
{
    new DataAccessCommand(
        "UPDATE dbo.Account SET Balance = Balance - @Amount WHERE Id = @Id",
        [
            new DataAccessParameter("@Amount", amount),
            new DataAccessParameter("@Id", sourceAccountId)
        ]),
    new DataAccessCommand(
        "UPDATE dbo.Account SET Balance = Balance + @Amount WHERE Id = @Id",
        [
            new DataAccessParameter("@Amount", amount),
            new DataAccessParameter("@Id", destinationAccountId)
        ])
};

await dataAccess.SetCommandListAsync(commands, cancellationToken);
```

## Timeouts and cancellation

Set a timeout per command with `TimeoutSeconds`:

```csharp
var command = new DataAccessCommand("EXEC dbo.RebuildPersonSummary")
{
    TimeoutSeconds = 120
};

await dataAccess.SetCommandAsync(command, cancellationToken);
```

The asynchronous APIs accept a `CancellationToken` for opening the connection and executing the command. A timeout limits database execution time; cancellation lets the caller stop work, for example when an HTTP request is aborted.

## Error handling

Provider exceptions are rethrown with diagnostic entries in `Exception.Data`:

- `DataAccess Query` contains the command text.
- `DataAccess Parameters` contains parameter names, values, and types when parameters exist.

This is useful for structured logging, but it also means exception details can contain sensitive values. Do not return raw database exceptions to clients, and apply the application's normal redaction rules when logging them.

## Practical guidance

- Prefer asynchronous methods in request-handling code.
- Always parameterize values received from users or external systems.
- Use `DbType` and `DBNull.Value` when a parameter's type or nullability is not obvious.
- Use `SetCommandListAsync` when multiple writes must succeed or fail together.
- Set `TimeoutSeconds` on long-running commands instead of changing the obsolete global `TimeOut` property.
- Use [`IEntityRepository`](~/articles/developer-guide/repositories/entities.md) when the operation should honor an `Element`, its metadata, or its `ConnectionId`.

## Further reading

- [Parameters and SQL safety](~/articles/developer-guide/data-access/parameters.md): Bind values, send SQL NULL, and use parameterized interpolation.
- [Stored procedures and output parameters](~/articles/developer-guide/data-access/stored-procedures.md): Execute procedures and retrieve output parameters.
- [Typed queries](~/articles/developer-guide/data-access/typed-queries.md): Generate model mappings, read typed results, and map nested objects.
