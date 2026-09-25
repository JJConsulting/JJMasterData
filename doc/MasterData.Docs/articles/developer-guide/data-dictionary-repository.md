# Data dictionary repository

`IDataDictionaryRepository` stores and retrieves `FormElement` definitions. A `FormElement` is the metadata that describes a generated CRUD: its table, fields, panels, actions, rules, relationships and UI options.

The repository manages definitions, not business records. Use [`IEntityRepository`](entity-repository.md) when you need to read or write rows from the tables described by those definitions.

## Inject and load a dictionary

Resolve `IDataDictionaryRepository` through dependency injection and identify a dictionary by its `FormElement.Name`:

```csharp
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

public sealed class PersonDictionaryService(
    IDataDictionaryRepository dictionaries)
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

The filesystem implementation stores one `<Name>.json` file per dictionary. This is useful when definitions should be reviewed and versioned with the application. See [Persistence and version control](../concepts/persistence.md) for deployment options.

Metadata storage is independent from the connection used by a dictionary's business table. See [Connections and repositories](connections.md) for connection selection.

## Initialize the storage

Create the required SQL table or filesystem directory during application startup. `UseMasterDataSeedingAsync` also initializes the audit-log structure:

```csharp
var app = builder.Build();

await app.UseMasterDataSeedingAsync();
```

If the host does not use the web initialization extension, call the repository directly from a scope:

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

`DataDictionaryFilter` can filter by name, mapped table and modification interval. Prefer the asynchronous methods in web requests.

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

The SQL repository caches dictionary names and loaded definitions when `MasterDataCoreOptions.EnableDataDictionaryCaching` is enabled. It is enabled by default. Writes performed through `IDataDictionaryRepository` invalidate the related cache entries.

Avoid editing the metadata table directly while the application is running. Those changes do not pass through the repository and therefore do not invalidate its in-memory cache.

## Replace the repository

Implement `IDataDictionaryRepository` when definitions come from another store, then replace the registration:

```csharp
builder.Services
    .AddJJMasterDataWeb(builder.Configuration)
    .WithDataDictionaryRepository<MyDataDictionaryRepository>();
```

A custom implementation must preserve the repository contract for structure initialization, complete-definition reads and writes, filtering, pagination, existence checks and deletion. Register any dependencies required by the implementation before building the application.
