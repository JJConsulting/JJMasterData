# Data access

`DataAccess` is JJMasterData's lightweight ADO.NET wrapper for executing SQL that is not represented by a data dictionary. It uses the database provider and connection string configured for JJMasterData, opens and disposes connections for each operation, and supports synchronous and asynchronous APIs.

Use `DataAccess` for custom queries, commands, and stored procedures. For CRUD operations based on an `Element`, prefer [`IEntityRepository`](~/articles/developer-guide/entity-repository.md), which applies the element metadata and resolves its configured connection.

The types in this guide are in the `JJMasterData.Commons.Data` namespace.

## Inject `DataAccess`

Register JJMasterData in the host and inject `DataAccess` into the service that needs it. `AddJJMasterDataCommons`, `AddJJMasterDataCore`, and `AddJJMasterDataWeb` register `DataAccess` as a scoped service.

```csharp
using JJMasterData.Commons.Data;

public sealed class PersonReportService(DataAccess dataAccess)
{
    public async Task<object?> GetNameAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        var command = new DataAccessCommand(
            "SELECT Name FROM dbo.Person WHERE Id = @Id",
            [new DataAccessParameter("@Id", personId)]);

        return await dataAccess.GetResultAsync(command, cancellationToken);
    }
}
```

The injected instance uses `JJMasterData:ConnectionString` and `JJMasterData:ConnectionProvider`. See [connections and repositories](~/articles/developer-guide/connections.md) for the complete connection configuration.

You can also create an instance directly when a query must use a connection that is not the JJMasterData default:

```csharp
var dataAccess = new DataAccess(
    connectionString,
    DataAccessProvider.PostgreSql);
```

## Choose the result shape

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

Equivalent synchronous methods are available without the `Async` suffix. In web applications, prefer the asynchronous methods and pass the request's `CancellationToken`.

### Read one value

`GetResultAsync` has the same semantics as ADO.NET's `ExecuteScalar`: it ignores every column and row except the first column of the first row.

```csharp
var command = new DataAccessCommand(
    "SELECT COUNT(*) FROM dbo.Person WHERE IsActive = @IsActive",
    [new DataAccessParameter("@IsActive", true)]);

var result = await dataAccess.GetResultAsync(command, cancellationToken);
var activePeople = Convert.ToInt32(result);
```

The result is `null` when the query returns no value. A database `NULL` may be returned as `DBNull.Value`, so convert or check the value according to the provider and query.

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

## Write data

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

## Parameters and SQL safety

Always put external values in `DataAccessParameter`. Do not concatenate or interpolate them into the SQL text.

```csharp
// Safe: the value is sent separately from the SQL text.
var command = new DataAccessCommand(
    "SELECT Id FROM dbo.Person WHERE Email = @Email",
    [new DataAccessParameter("@Email", email)]);
```

Parameter markers are provider-specific. SQL Server, SQLite, MySQL, and PostgreSQL commonly use names such as `@Email`; Oracle commands commonly use names such as `p_email`. Match the syntax expected by the configured provider.

Specify `DbType` when type inference is ambiguous, especially for nullable values, fixed sizes, decimals, and provider-sensitive string types:

```csharp
var nicknameParameter = new DataAccessParameter(
    "@Nickname",
    nickname is null ? DBNull.Value : nickname,
    DbType.String,
    100);
```

`DataAccessParameter` requires `DBNull.Value`, not C# `null`, to send a SQL `NULL`.

For compact parameterized queries, `DataAccessCommand` can be created explicitly from a `FormattableString`. The values become `@p0`, `@p1`, and so on; they are not inserted into the SQL text.

```csharp
DataAccessCommand command = (DataAccessCommand)$"""
    SELECT Id, Name
    FROM dbo.Person
    WHERE CreatedAt >= {startDate} AND IsActive = {true}
    """;
```

Use this form only with providers that accept `@` parameter markers. Keep SQL identifiers such as table, column, and sort-direction names out of interpolated input; parameters protect values, not identifiers.

## Stored procedures and output parameters

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
- Use [`IEntityRepository`](~/articles/developer-guide/entity-repository.md) when the operation should honor an `Element`, its metadata, or its `ConnectionId`.
