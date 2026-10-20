# Add application expression functions

Several dictionaries need the same rounding rule, and some actions should be visible only to members of an application role. Register small functions in the existing `exp:` provider instead of repeating calculations in each dictionary.

## Register a calculation

After registering JJMasterData, extend its options without replacing the built-in function dictionary:

```csharp
using System.Globalization;
using JJMasterData.Core.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;
using NCalc;
using NCalc.Handlers;

builder.Services.PostConfigure<MasterDataCoreOptions>(options =>
{
    options.ExpressionContext.Functions["RoundUpToStep"] = call =>
    {
        if (call.Count != 2)
            throw new ArgumentException("RoundUpToStep requires a value and a positive step.");

        var value = Convert.ToDecimal(call.Evaluate(0), CultureInfo.InvariantCulture);
        var step = Convert.ToDecimal(call.Evaluate(1), CultureInfo.InvariantCulture);
        if (step <= 0)
            throw new ArgumentOutOfRangeException(nameof(step));

        return Math.Ceiling(value / step) * step;
    };
});
```

For a numeric `Quantity` field, use a trigger such as `exp:RoundUpToStep('{Quantity}', 5)`. For example, 12 becomes 15. Define how missing values should behave; this implementation uses `Convert.ToDecimal`, which converts null to zero and rejects nonnumeric strings.

`call.Evaluate(index)` evaluates that argument in the current expression context. Validate argument count and return a value of the type expected by the field or action. Keep the function deterministic and inexpensive: expressions can run repeatedly while rendering and reloading editors.

## Resolve request-scoped context

The built-in provider puts its current `IServiceProvider` into `call.Context.Parameters["ServiceProvider"]`. Resolve services inside the function invocation rather than capturing a user or scoped dependency during options configuration:

```csharp
using Microsoft.AspNetCore.Http;

builder.Services.PostConfigure<MasterDataCoreOptions>(options =>
{
    options.ExpressionContext.Functions["HasApplicationRole"] = call =>
    {
        if (call.Count != 1)
            return false;

        var services = (IServiceProvider)call.Context.Parameters["ServiceProvider"]!;
        var user = services.GetRequiredService<IHttpContextAccessor>().HttpContext?.User;
        var role = Convert.ToString(call.Evaluate(0));

        return user?.Identity?.IsAuthenticated == true &&
               !string.IsNullOrWhiteSpace(role) && user.IsInRole(role);
    };
});
```

A visible expression can now use `exp:HasApplicationRole('PlanningManager')`. The function returns false without an authenticated HTTP user, including execution contexts without a browser request. Ensure the host maps role claims correctly for `ClaimsPrincipal.IsInRole`.

This only controls rendering. The save endpoint, API and data-access layer still enforce the corresponding policy. Avoid network calls or asynchronous database work in these synchronous functions. Use a form handler or application service when a decision requires I/O.

## Check the result

Test the calculation with exact multiples, fractional values, missing input and invalid steps. Evaluate role-based visibility as two different authenticated users and without an HTTP context. Confirm that default functions such as `coalesce()` still work and that direct calls to the protected operation enforce the same permissions.

See [Expressions](../concepts/expressions.md) for providers, placeholders and the boundary between calculations and validation.
