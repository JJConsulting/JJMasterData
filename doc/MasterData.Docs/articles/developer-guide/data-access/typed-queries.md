# Typed queries

Use [`DataAccess`](index.md) from the `JJMasterData.Commons.Data` namespace. The examples assume a configured `dataAccess` instance; see [Basic usage](index.md#basic-usage).

Install `JJMasterData.Data.SourceGenerators` alongside `JJMasterData.Commons`:

```xml
<PackageReference Include="JJMasterData.Data.SourceGenerators" PrivateAssets="all" />
```

Add `[DataAccessModel]` to a partial class and use `GetAsync<T>` or `GetListAsync<T>`:

```csharp
using JJMasterData.Commons.Data;

[DataAccessModel]
public partial class Product
{
    [DataAccessColumn("product_id")]
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
}

// Inside an application method:
var command = new DataAccessCommand(
    "SELECT Id AS product_id, Description, Price FROM Product");

Product? product = await dataAccess.GetAsync<Product>(command, cancellationToken);
List<Product> products = await dataAccess.GetListAsync<Product>(command, cancellationToken);
```

`GetAsync<T>` returns the first row or `null`. `GetListAsync<T>` returns all rows or an empty list. Both read only the first result set.

## Mapping rules

- Column names match property names, ignoring case. For aliases, use `[DataAccessColumn]` or `[JsonPropertyName]`; `DataAccessColumn` takes priority. Overrides inherit these attributes.
- Extra columns are ignored; missing columns keep property initializers. Duplicate matched columns throw `DataException`.
- SQL `NULL` requires a nullable property, including `string?`.
- Supported types: strings, booleans, characters, numeric primitives, `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, enums, and nullable variants. Unsafe conversions throw `DataException`.
- Use `[DataAccessIgnore]` to skip a property, including computed, `required`, `init`, or unsupported properties. Ignored properties keep their constructor or initializer values.

## Nested models

Properties whose type has `[DataAccessModel]` read from the same row using that model's column names:

```csharp
[DataAccessModel]
public partial class Order
{
    [DataAccessColumn("order_id")]
    public int Id { get; set; }
    public Product Product { get; set; } = null!;

    [DataAccessIgnore]
    public string Summary => $"{Id}: {Product.Description}";
}

// Each row supplies order_id, product_id, Description and Price.
var orders = await dataAccess.GetListAsync<Order>(new DataAccessCommand(
    "SELECT o.Id AS order_id, p.Id AS product_id, p.Description, p.Price " +
    "FROM Orders o JOIN Product p ON p.Id = o.ProductId"), cancellationToken);
```

Nested objects are created even when their columns are missing, preserving their property initializers. Cycles between models produce a generator diagnostic.

## Requirements

- Models must be concrete, non-generic partial classes with a public or internal parameterless constructor. Containing types must also be partial and non-generic.
- Mapped properties must be public with accessible setters. Collections, indexers, static properties, `required`, and `init` are unsupported unless ignored.

The generator implements `IDataAccessModel<T>`, which the typed query methods require. It resolves column positions once per query and reads rows directly into models. 
You write the SQL; cancellation, timeouts, and output parameters work as usual.

[Data access overview](index.md)
