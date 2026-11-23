# Data dictionary repository

`IDataDictionaryRepository` stores and retrieves `FormElement` definitions. A `FormElement` is the metadata that describes a generated CRUD: its table, fields, panels, actions, rules, relationships and UI options.

The repository manages definitions, not business records. Use [`IEntityRepository`](entities.md) when you need to read or write rows from the tables described by those definitions.

## Inject and load a dictionary

Resolve `DataDictionaryService` through dependency injection to read and write definitions with cache management. Identify a dictionary by its `FormElement.Name`:

```csharp
using JJMasterData.Core.DataDictionary.Services;

public sealed class PersonDictionaryService(
    DataDictionaryService dictionaries)
{
    public async ValueTask<string> GetTableNameAsync()
    {
        var element = await dictionaries.GetFormElementAsync("Person");
        return element.TableName;
    }
}
```

Use `ExistsAsync` first when the dictionary is optional:

```csharp
if (!await dictionaries.ExistsAsync("Person"))
    return;

var element = await dictionaries.GetFormElementAsync("Person");
```

Both `GetFormElement` and `GetFormElementAsync` throw `FormElementNotFoundException` (derived from `JJMasterDataException`) when the dictionary does not exist. They do not return `null`.

## Choose the storage provider

JJMasterData registers the SQL repository by default. You can select the provider explicitly through the builder returned by `AddJJMasterDataWeb`:

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Web.Configuration;

builder.Services
    .AddJJMasterDataWeb(builder.Configuration)
    .WithDatabaseDataDictionary();
```

The SQL implementation stores serialized definitions in the configured data dictionary table. Its default name is `tb_masterdata`. The table uses the application's entity provider and default connection unless configured otherwise.

For file-based definitions, configure a writable folder:

```csharp
builder.Services
    .AddJJMasterDataWeb(builder.Configuration)
    .WithFileSystemDataDictionary(options =>
        options.FolderPath = Path.Combine(
            builder.Environment.ContentRootPath,
            "Dictionaries"));
```

The filesystem implementation stores one `<Name>.json` file per dictionary. This is useful when definitions should be reviewed and versioned with the application. See [Persistence and version control](../../concepts/persistence.md) for deployment options.

Metadata storage is independent from the connection used by a dictionary's business table. See [Connections and repositories](index.md) for connection selection.

## Initialize the storage

Create the required SQL table or filesystem directory during application startup. `UseMasterDataSeedingAsync` also initializes the audit-log structure:

```csharp
var app = builder.Build();

await app.UseMasterDataSeedingAsync();
```

If the host does not use the web initialization extension, call the service from a scope:

```csharp
await dictionaries.CreateStructureIfNotExistsAsync();
```

The database identity running this operation needs permission to create the metadata table. The filesystem identity needs permission to create and write to the configured directory.

## Read definitions
Use the summary query for management screens or catalog endpoints. It returns the name, mapped table, description, API status and modification timestamp without exposing the full definition:

```csharp
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Structure;

var filter = new DataDictionaryFilter
{
    Name = "Person"
};

var orderBy = new OrderByData();
orderBy.AddOrReplace("Name", OrderByDirection.Asc);

var page = await dictionaries.GetFormElementInfoListAsync(
    filter,
    orderBy,
    recordsPerPage: 20,
    currentPage: 1);
```

`DataDictionaryFilter` can filter by name, mapped table, modification interval and JSON content (`Json`, shown as “Filter for anything” in the editor). Prefer the asynchronous methods in web requests.

## Save and delete definitions

`InsertOrReplaceAsync` creates a dictionary or replaces the complete stored definition with the same name:

```csharp
var element = await dictionaries.GetFormElementAsync("Person");
element.Info = "People registered by the application";

await dictionaries.InsertOrReplaceAsync(element);
```

You can also persist multiple definitions in one call:

```csharp
await dictionaries.InsertOrReplaceAsync(elements);
```

Delete a definition by name:

```csharp
await dictionaries.DeleteAsync("Person");
```

> [!WARNING]
> Saving or deleting dictionary metadata does not create, alter or remove the corresponding business table. Apply database schema changes separately.

Treat a loaded `FormElement` as a complete document. Repository writes do not merge individual fields and do not provide optimistic concurrency checks, so concurrent changes to the same definition can overwrite each other.

## Caching

`DataDictionaryService` caches dictionary names and loaded definitions for the configured repository when `MasterDataCoreOptions.EnableDataDictionaryCaching` is enabled. It is enabled by default. Writes and deletions through the service invalidate related entries even when caching is disabled for the writing scope. Generated SQL scripts are cached separately and are also invalidated when affected.

Writes directly through `IDataDictionaryRepository`, or changes made outside the application to the metadata table or JSON files, bypass service cache invalidation. Use `DataDictionaryService` for application reads and writes.

## Replace the repository

Implement `IDataDictionaryRepository` when definitions come from another store, then replace the registration:

```csharp
builder.Services
    .AddJJMasterDataWeb(builder.Configuration)
    .WithDataDictionaryRepository<MyDataDictionaryRepository>();
```

A custom implementation must preserve the repository contract for structure initialization, complete-definition reads and writes, filtering, pagination, existence checks and deletion. Register any dependencies required by the implementation before building the application.
