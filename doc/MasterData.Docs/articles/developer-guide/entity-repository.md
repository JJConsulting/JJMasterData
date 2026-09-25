# Entity repository

`IEntityRepository` reads and writes business records using an `Element` or `FormElement`. The element supplies the table, fields, keys, connection and provider-specific settings used by each operation.

Use [`IDataDictionaryRepository`](data-dictionary-repository.md) to load the metadata, then pass it to the entity repository:

```csharp
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

public sealed class PersonService(
    IDataDictionaryRepository dictionaries,
    IEntityRepository entities)
{
    public async Task<Dictionary<string, object?>> GetAsync(int id)
    {
        var element = await dictionaries.GetFormElementAsync("Person");

        return await entities.GetFieldsAsync(element,
            new Dictionary<string, object> { ["Id"] = id });
    }
}
```

The primary-key dictionary must use database field names. If no matching record exists, `GetFieldsAsync` returns an empty dictionary.

## Query records

Use `GetDictionaryListAsync` for a list of rows. Pass `EntityParameters` when you need filters, ordering or pagination:

```csharp
using JJMasterData.Commons.Data.Entity.Repository;

var element = await dictionaries.GetFormElementAsync("Person");

var orderBy = new OrderByData()
    .AddOrReplace("Name", OrderByDirection.Asc);

var parameters = new EntityParameters
{
    Filters = new Dictionary<string, object?>
    {
        ["IsActive"] = true
    },
    OrderBy = orderBy,
    CurrentPage = 1,
    RecordsPerPage = 25
};

var people = await entities.GetDictionaryListAsync(element, parameters);
```

Use `GetDictionaryListResultAsync` when the caller also needs the total number of matching records:

```csharp
var result = await entities.GetDictionaryListResultAsync(
    element,
    parameters,
    recoverTotalOfRecords: true);

var rows = result.Data;
var total = result.TotalOfRecords;
```

Set `recoverTotalOfRecords` to `false` when the total is not displayed. This avoids the additional count work required by some providers.

Other useful read methods include:

| Method | Use |
| --- | --- |
| `GetFieldsAsync` | Load one record from its primary-key values. |
| `GetDictionaryListAsync` | Return rows as dictionaries. |
| `GetDictionaryListResultAsync` | Return rows plus the total record count. |
| `GetDataTableAsync` | Return an ADO.NET `DataTable`. |
| `GetCountAsync` | Count rows matching a field-value filter. |
| `GetListFieldsAsTextAsync` | Serialize selected results as delimited text. |

Field names used in filters and ordering must exist in the supplied element. Values are passed to the provider as parameters.

## Write records

Use the method that expresses the intended operation:

```csharp
var values = new Dictionary<string, object?>
{
    ["Id"] = 42,
    ["Name"] = "Ada Lovelace",
    ["IsActive"] = true
};

await entities.InsertAsync(element, values);
await entities.UpdateAsync(element, values);

await entities.DeleteAsync(element,
    new Dictionary<string, object> { ["Id"] = 42 });
```

`SetValuesAsync` performs an insert or update according to the element's primary-key values and returns the resulting `CommandOperation`:

```csharp
var operation = await entities.SetValuesAsync(element, values);
```

For bulk operations, use the overloads that receive `IEnumerable<Dictionary<string, object?>>` instead of issuing one repository call per row.

> [!IMPORTANT]
> Direct `IEntityRepository` writes bypass `FormService`. They do not run field validation, dictionary rules, form events or JJMasterData audit processing.

Use the form-service workflow when a write must behave like a submission from the generated UI. See [Form events](form-events.md) for the hooks executed by that workflow.

## Connections

The repository resolves the database from `element.ConnectionId`. A null connection ID uses the default JJMasterData connection; a configured ID selects the corresponding additional connection. See [Connections and repositories](connections.md).

Do not build an element only to switch databases for arbitrary SQL. For queries that are not represented by dictionary metadata, use [`DataAccess`](data-access.md).

## Inspect and create database structures

`IEntityRepository` can inspect an existing table and produce an `Element`:

```csharp
var element = await entities.GetElementFromTableAsync(
    schemaName: "dbo",
    tableName: "Person");
```

It also exposes schema helpers such as `TableExistsAsync`, `GetCreateTableScript`, `GetAlterTableScriptAsync` and `CreateDataModelAsync`.

```csharp
if (!await entities.TableExistsAsync("dbo", "Person"))
    await entities.CreateDataModelAsync(element);
```

`CreateDataModelAsync` performs database schema changes. Run it with an appropriately privileged connection and review generated scripts before using it against production databases.

## Replace the repository

Applications with a custom persistence layer can replace the default implementation:

```csharp
builder.Services
    .AddJJMasterDataWeb(builder.Configuration)
    .WithEntityRepository<MyEntityRepository>();
```

A custom repository must implement the complete `IEntityRepository` contract, including queries, writes, schema operations and connection-aware commands expected by JJMasterData internals.
