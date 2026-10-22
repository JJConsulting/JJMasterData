# Parameters and SQL safety

Use [`DataAccess`](index.md) from the `JJMasterData.Commons.Data` namespace. The examples assume a configured `dataAccess` instance; see [Basic usage](index.md#basic-usage).

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

[Data access overview](index.md)
