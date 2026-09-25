# Form events

Register an `IFormEventHandler` with the exact dictionary name. The default lifetime is transient; constructor injection is supported.

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.Events.Args;

builder.Services.AddFormEventHandler<PersonEvents>("Person");

public sealed class PersonEvents : IFormEventHandler
{
    public ValueTask OnBeforeInsertAsync(object sender, FormBeforeActionEventArgs args)
    {
        if (!args.Values.TryGetValue("Name", out var name) ||
            string.IsNullOrWhiteSpace(name?.ToString()))
            args.Errors["Name"] = "Enter a name.";

        return ValueTask.CompletedTask;
    }
}
```

Add errors to `args.Errors` to stop persistence; use field names as keys to associate messages with editors. `Values` contains the values about to be written. Apply the equivalent update/import hook when the same rule must cover those operations.

| Hook | Use |
| --- | --- |
| `OnFormElementLoadAsync` | Adjust the loaded dictionary before rendering. |
| `OnBeforeInsertAsync`, `OnBeforeUpdateAsync`, `OnBeforeDeleteAsync`, `OnBeforeImportAsync` | Validate or change submitted values. |
| `OnAfterInsertAsync`, `OnAfterUpdateAsync`, `OnAfterDeleteAsync` | React after persistence or set `UrlRedirect`. |

For grid rendering and data loading, see [grid events](grid-events.md).

## Document customized fields

Implement `GetCustomizedFields()` to identify fields affected by a handler. The dictionary editor displays an information alert for these fields:

```csharp
public IEnumerable<string> GetCustomizedFields()
{
    yield return "Name";
}
```

Use form events for business logic that needs injected services and should also apply through the Web API. For validations configured in the dictionary editor, use [validation rules](../concepts/rules.md).


## Share validation between operations

Insert, update and import have separate before hooks. A check in `OnBeforeInsertAsync` alone does not cover file imports. Reuse the same validation method from the hooks that need it:

```csharp
public ValueTask OnBeforeUpdateAsync(object sender, FormBeforeActionEventArgs args)
    => OnBeforeInsertAsync(sender, args);

public ValueTask OnBeforeImportAsync(object sender, FormBeforeActionEventArgs args)
    => OnBeforeInsertAsync(sender, args);
```

Add these methods to `PersonEvents` above. Mutate `args.Values` to adjust submitted values and `args.Errors` to reject an operation. An after hook runs after persistence; use a before hook for validation that must prevent the write.

The import job resolves the registered handler, calls `OnFormElementLoadAsync`, and attaches `OnBeforeImportAsync` plus the after-insert/update/delete hooks. It does not attach the ordinary before-insert/update/delete hooks. The repository's reported operation determines which after hook runs.

## Registration and lifetime

Registrations use keyed dependency injection with the dictionary name as the key. Use the same spelling as `FormElement.Name`. Register form and grid interfaces separately even when a class implements both.

The optional `ServiceLifetime` argument supports transient (default), scoped and singleton. Choose a lifetime compatible with injected dependencies. A background import resolves services in its execution context; do not depend on an active browser request inside its handler.

`FormElementLoadEventArgs.FormElement` exposes the loaded metadata. `FormAfterActionEventArgs.Values` exposes the operation values and `UrlRedirect` supplies a redirect to callers that handle it. A background import does not navigate the user's browser through this property.

Direct `IEntityRepository` writes bypass these form-service hooks. See [entity repository](entity-repository.md) and [validation rules](../concepts/rules.md#rule-syntax-and-operation-flags).
