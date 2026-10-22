# Validate business operations

A resource booking must end after it starts, and a referenced resource must not be deleted. Use a form handler for useful validation messages, while preserving database constraints for concurrent writers.

## Validate related fields

Register the handler for the `Booking` dictionary. Its `StartsAt` and `EndsAt` fields are required date/time fields:

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;

builder.Services.AddFormEventHandler<BookingEvents>("Booking");

public sealed class BookingEvents : IFormEventHandler
{
    private static ValueTask Validate(FormBeforeActionEventArgs args)
    {
        if (args.Values.TryGetValue("StartsAt", out var start) && start is DateTime startsAt &&
            args.Values.TryGetValue("EndsAt", out var end) && end is DateTime endsAt &&
            endsAt <= startsAt)
        {
            args.Errors["EndsAt"] = "The end must be after the start.";
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args)
        => Validate(args);

    public ValueTask OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args)
        => Validate(args);

    public ValueTask OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args)
        => Validate(args);

    public IEnumerable<string> GetCustomizedFields() => ["StartsAt", "EndsAt"];
}
```

This example expects typed `DateTime` values from date/time fields. Required/type validation must also be configured in the dictionary. A partial-write endpoint must first merge stored values or explicitly reject missing dates; the cross-field test above cannot compare a missing value.

For overlaps, call an injected application service from these same hooks. Pass the resource, date range and current booking key so an update excludes itself. Reject an overlap by adding a field error. Protect against concurrent overlapping submissions in the database as well; a before-hook query and a later write are separate operations.

## Reject deletion of referenced data

For a `Resource` dictionary, implement and register this application contract with your chosen database access:

```csharp
public interface IResourceUsage
{
    Task<bool> HasBookingsAsync(int resourceId);
}

public sealed class ResourceEvents(IResourceUsage usage) : IFormEventHandler
{
    public async ValueTask OnBeforeDeleteAsync(object sender, FormBeforeActionEventArgs args)
    {
        var id = Convert.ToInt32(args.Values["Id"]);
        if (await usage.HasBookingsAsync(id))
            args.Errors["Id"] = "Remove or reassign the bookings before deleting this resource.";
    }
}

builder.Services.AddFormEventHandler<ResourceEvents>(
    "Resource", Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped);
```

Keep the foreign key restriction even with this friendly check. Another request can create a reference between checking and deleting. Do not delete related data or create compensating records during validation: a later validation can still reject the operation.

An after-event cannot reject an already completed write. Use it for follow-up behavior whose failure semantics you understand. If a write and a side effect must commit together, place them in a shared transaction or use an application outbox design.

## Check the result

Exercise insertion, update and import with an invalid range. Verify that a valid update does not overlap itself. Try deleting a referenced resource and confirm that it remains. Repeat through the Web API when enabled, and remember that direct repository writes and SQL actions bypass these hooks.

See [Form events](../developer-guide/customizations/form-events.md) for registration and the import-specific lifecycle.
