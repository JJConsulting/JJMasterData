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

## Compile-time model materialization

Install `JJMasterData.DataAccess.SourceGenerator` alongside `JJMasterData.Commons` (the Commons version must include the typed query API). The generator requires a Roslyn 4.14 or newer compiler (the repository uses .NET 10). The generator package contains a build analyzer under `analyzers/dotnet/cs`; Roslyn is not a runtime dependency. Mark its reference as private so applications control whether to enable generation:

```xml
<PackageReference Include="JJMasterData.DataAccess.SourceGenerator" Version="5.0.12" PrivateAssets="all" />
```

Models do not need `partial`. Put the model namespace in scope to use its generated extensions:

```csharp
using JJMasterData.Commons.Data;

[DataAccessModel]
public class Product
{
    [DataAccessColumn("product_id")]
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
}
```

Query it from an application method:

```csharp
var command = new DataAccessCommand(
    "SELECT Id AS product_id, Description, Price FROM Product");
var products = await dataAccess.GetProductListAsync(command, cancellationToken);
var product = await dataAccess.GetProductAsync(command, cancellationToken);
```

The first-row method returns `null` for an empty result, and the list method returns an empty list. Only the first result set is mapped. Unlike `GetDictionaryListAsync`, these methods construct models directly from `DbDataReader`, without an intermediate dictionary or table. Ordinals are bound once for each query, using ordinal case-insensitive column matching. Extra columns are ignored; missing columns preserve property initializers. Duplicate matched SQL column names fail with `DataException`.

Supported properties are `string`, `bool`, `char`, all C# integer types, `float`, `double`, `decimal`, `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, enums, and their nullable variants. SQL `NULL` sets nullable properties to `null`; it throws `DataException` for non-nullable properties, including non-nullable strings. For projects with nullable reference types disabled, strings are treated as non-nullable; enable nullable annotations to declare `string?`.

Exact provider types use a direct type check. Numeric conversions accept numeric provider values, check integral range, and reject fractional values assigned to integer properties. Numeric enum values use the enum's underlying integer type; undefined enum values remain allowed. Numeric `0`/`1` can map to `bool`, and a one-character string can map to `char`. Arbitrary values are never converted to strings. Conversion failures identify the column and expected type and preserve the normal `DataAccess Query`/`DataAccess Parameters` exception context.

Canonical `D`-format strings can map to `Guid`; invariant ISO date strings can map to `DateTime` and `DateTimeOffset` (offset required), and constant-format (`c`) duration strings can map to `TimeSpan`. SQLite returns integers as `long`, real expressions as `double`, and stored decimals and temporal values as strings. Decimal properties additionally accept invariant decimal strings with an optional leading sign and decimal point. See [Microsoft.Data.Sqlite data types](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types). Floating-point source values retain their provider precision; use an appropriate SQL type/provider for exact decimals. Provider-specific wrapper types and binary GUID representations are unsupported; cast them in SQL to a supported representation. Other providers use the same ADO.NET path but need verification against their actual returned types. Provider factory discovery is unchanged; with Microsoft.Data.Sqlite, pass `SqliteFactory.Instance` explicitly as in the runnable example.

Models must be accessible, concrete, non-generic classes with an accessible parameterless constructor and public properties with public or internal setters. Inherited properties are mapped. Nested objects, collections, indexers, static properties, inaccessible setters, `required` members and `init` setters are rejected. The generator does not produce SQL or change the data dictionary or CRUD system. Queries retain the existing per-operation connection ownership, timeout, cancellation and output-parameter behavior. The existing query API does not expose an external connection or transaction.

Generation errors appear as build diagnostics:

| Diagnostic | Meaning |
| --- | --- |
| JJMDG001 | Invalid, inaccessible, abstract or generic model |
| JJMDG002 | No accessible parameterless constructor |
| JJMDG003 | Unsupported property type |
| JJMDG004 | Property/member cannot be assigned safely |
| JJMDG005 | Duplicate mapped column names |
| JJMDG006 | Conflicting generated type or method names |
| JJMDG007 | Invalid attribute configuration |

Classes with identical names in different namespaces, and names such as `Product`/`ProductList`, can create ambiguous extension calls. Set a unique suffix using `[DataAccessModel(ExtensionName = "SalesProduct")]`; this generates `GetSalesProductAsync` and `GetSalesProductListAsync` in the model namespace.

For debugging, optionally emit generated files under `obj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Run the SQLite example with `dotnet run --project example/DataAccess.SourceGenerator`. Compare dictionary, generated and handwritten materialization with `dotnet run -c Release --project benchmarks/DataAccess.SourceGenerator -- --filter '*'`. BenchmarkDotNet is used only by the benchmark project; it measures time, allocation and GC behavior for 1, 1,000 and 10,000 rows. [Microsoft.Data.Sqlite async methods execute synchronously internally](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async), so this benchmark measures materialization rather than network/database concurrency.
