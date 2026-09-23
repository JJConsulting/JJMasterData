# Grid events

Implement `IGridEventHandler` for behavior specific to a grid. Register it for the dictionary that the grid renders:

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Core.UI.Events.Args;

builder.Services.AddGridEventHandler<PersonGridEvents>("Person");

public sealed class PersonGridEvents : IGridEventHandler
{
    public ValueTask OnFilterLoadAsync(object sender, GridFilterLoadEventArgs args)
    {
        args.Filters["IsActive"] = true;
        return ValueTask.CompletedTask;
    }
}
```

This example assumes the dictionary contains `IsActive`. The handler adds a grid filter; enforce access permissions in the application's authorization and data-access paths as well.

## Example: filter by the current user

Inject the current user (or an application service that resolves it) when the default filter depends on the logged-in user. Use a scoped handler when the handler has scoped dependencies:

```csharp
public sealed class OrderGridEvents(IUserContext userContext) : IGridEventHandler
{
    public ValueTask OnFilterLoadAsync(object sender, GridFilterLoadEventArgs args)
    {
        args.Filters["CustomerId"] = userContext.CustomerId;
        args.Filters["Status"] = "Open";
        return ValueTask.CompletedTask;
    }
}

builder.Services.AddGridEventHandler<OrderGridEvents>(
    "Order",
    ServiceLifetime.Scoped);
```

The filter is applied when the grid loads. It is not a substitute for authorization: the same restriction must also be enforced by the repository or service used by other entry points.

## Available hooks

| Hook | Arguments and purpose |
| --- | --- |
| `OnFilterLoadAsync` | Change entries in `Filters`. |
| `OnDataLoadAsync` | Inspect filters, ordering and pagination; supply `DataSource` and `TotalOfRecords`. |
| `OnRenderCell` | Inspect `Field`, `DataRow` and `Sender`; set `HtmlResult` to customize cell content. |
| `OnRenderSelectedCell` | Customize the selection cell. |
| `OnRenderAction` | Customize a row action. |
| `OnRenderToolbarAction` | Customize a toolbar action. |
| `OnRenderRow` | Modify the row's `HtmlBuilder`, using `RowValues` as context. |

`OnFilterLoadAsync` returns `ValueTask`; `OnDataLoadAsync` returns `Task`. Rendering hooks are synchronous. Keep database work out of per-cell rendering hooks to avoid repeating queries for every cell.

## Supplying data

`GridDataLoadEventArgs` exposes `Filters`, `OrderBy`, `RecordsPerPage` and `CurrentPage`. When supplying a custom `DataSource`, also set `TotalOfRecords` to the full matching count so pagination represents the result correctly. Each row is a dictionary keyed by field name.

Filtering and paging must be implemented by the custom data source. A list containing only the current page cannot supply the full count by its own `Count`.

For example, an in-memory data source can apply the grid state before assigning the result:

```csharp
public sealed class ProductGridEvents(IProductReader products) : IGridEventHandler
{
    public async Task OnDataLoadAsync(object sender, GridDataLoadEventArgs args)
    {
        var query = products.Query();

        if (args.Filters.TryGetValue("Category", out var category) && category is string value)
            query = query.Where(product => product.Category == value);

        var matchingProducts = await query.ToListAsync();
        args.TotalOfRecords = matchingProducts.Count;

        var page = matchingProducts
            .Skip(args.CurrentPage * args.RecordsPerPage)
            .Take(args.RecordsPerPage);

        args.DataSource = page.Select(product => new Dictionary<string, object?>
        {
            ["Id"] = product.Id,
            ["Name"] = product.Name,
            ["Category"] = product.Category,
            ["Stock"] = product.Stock
        }).ToList();
    }
}
```

For a database-backed source, prefer applying `Where`, `OrderBy`, `Skip` and `Take` to the database query and run a separate `CountAsync` for `TotalOfRecords`. The exact page calculation depends on how the host represents `CurrentPage`; verify it against the grid configuration before copying the example.

## Example: status badge in a cell

`OnRenderCell` runs for each rendered cell. Check the field name before changing the result and use the row dictionary for the value:

```csharp
public sealed class OrderGridEvents : IGridEventHandler
{
    public void OnRenderCell(object? sender, GridCellEventArgs args)
    {
        if (args.Field.Name != "Status" ||
            !args.DataRow.TryGetValue("Status", out var value))
            return;

        var status = Convert.ToString(value);
        var cssClass = status switch
        {
            "Open" => "text-bg-success",
            "Blocked" => "text-bg-danger",
            _ => "text-bg-secondary"
        };

        args.HtmlResult = new HtmlBuilder(
            $"<span class=\"badge {cssClass}\">{HtmlEncoder.Default.Encode(status)}</span>",
            encode: false);
    }
}
```

When creating raw HTML, encode values that came from the database or the user. For text-only changes, using the normal field rendering is safer than replacing `HtmlResult`.

## Example: disable selection for a row

The selected-cell hook receives the row values and the checkbox component. This is useful when only eligible rows may be selected for a bulk operation:

```csharp
public void OnRenderSelectedCell(object? sender, GridSelectedCellEventArgs args)
{
    if (args.DataRow.TryGetValue("Status", out var status) &&
        Convert.ToString(status) == "Closed")
    {
        args.CheckBox.Enabled = false;
    }
}
```

If the host does not expose a disabled state, enforce the same rule when processing the bulk action.

## Example: customize a row action or toolbar action

Actions expose their name and the values associated with the row. Set `HtmlResult` when the action needs a small visual cue, such as a confirmation icon:

```csharp
public void OnRenderAction(object? sender, ActionEventArgs args)
{
    if (args.ActionName == "Delete")
        args.HtmlResult = "<i class=\"bi bi-trash\" aria-hidden=\"true\"></i>";
}

public void OnRenderToolbarAction(object? sender, GridToolbarActionEventArgs args)
{
    if (args.ActionName == "Export")
        args.HtmlResult = "<span class=\"me-1\">↓</span> Export";
}
```

Do not rely on hiding an action for security. Check permissions again when the action is executed.

## Example: highlight an entire row

Use `OnRenderRow` when the condition applies to the row rather than to a single field:

```csharp
public void OnRenderRow(object? sender, GridRowEventArgs args)
{
    if (args.RowValues.TryGetValue("Stock", out var stock) &&
        stock is int quantity && quantity == 0)
    {
        args.HtmlBuilder.WithCssClass("table-warning");
    }
}
```

Keep row and cell hooks deterministic and cheap. Resolve related data in `OnDataLoadAsync` or project it into the data source; a database query inside `OnRenderCell` can become one query per cell.

## Registration scope

The default handler lifetime is transient. `AddGridEventHandler<T>` also accepts scoped or singleton lifetimes. If a class implements both `IGridEventHandler` and `IFormEventHandler`, register both interfaces explicitly.

Grid hooks customize the UI. Use [form events](form-events.md) for validation that must run during persistence and import. Export has its own repository-based pipeline; do not assume a grid rendering customization changes exported values.
