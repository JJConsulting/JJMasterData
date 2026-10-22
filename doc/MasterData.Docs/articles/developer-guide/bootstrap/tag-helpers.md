# Bootstrap Tag Helpers (`JJConsulting.Html.Bootstrap.TagHelpers`)

`JJConsulting.Html.Bootstrap.TagHelpers` exposes the Bootstrap components as declarative elements in ASP.NET Core Razor views. Tag Helpers accept attributes and, where supported, Razor child content, then render the corresponding component as HTML.

Install the package:

```bash
dotnet add package JJConsulting.Html.Bootstrap.TagHelpers
```

The host application must load the Bootstrap CSS and JavaScript needed by the rendered components. The package does not bundle those assets. See [Custom Bootstrap](custom-bootstrap.md) for JJMasterData stylesheet configuration and [Bootstrap components](components.md) for component behavior and compatibility settings.

## Register the Tag Helpers

Add the assembly to `_ViewImports.cshtml` so the helpers are available throughout the Razor views under that folder:

```razor
@addTagHelper *, JJConsulting.Html.Bootstrap.TagHelpers
```

You can place this directive in a narrower `_ViewImports.cshtml` if only part of the application uses the helpers.

## Alerts and cards

The `alert` helper supports a title, color, close button, a message, a list of messages, and nested Razor content:

```razor
<alert title="Saved" color="Success" show-close-button="true">
    <p>Your changes were saved.</p>
</alert>
```

The `card` helper passes its child content to the card component:

```razor
<card title="Customer" color="Primary" layout="Panel">
    <p>Account details appear here.</p>
</card>
```

Enum attributes use the corresponding model names, such as `Primary`, `Warning`, or `Panel`. Razor encodes ordinary child text. `jj-title` passes its title and subtitle to `JJTitle`, which currently writes them as raw HTML; use trusted values there. See the component documentation for other properties that render strings as raw markup.

## Titles, links, and icons

Use `jj-title` for page headings and actions. `link-button` can generate a URL with MVC action and route attributes, or render as a button. The `icon` helper renders a Font Awesome icon.

```razor
<jj-title title="Customers" subtitle="Manage customer accounts" size="H1" />

<link-button asp-controller="Customers"
             asp-action="Create"
             text="Add customer"
             icon="Plus"
             color="Primary" />

<icon icon="InfoCircle" tooltip="Customer records are updated nightly." />
```

`link-button` supports `asp-route-*` attributes for route values. Its `type`, `enabled`, `show-as-button`, `css-class`, `tooltip`, and `on-client-click` attributes configure the generated `JJLinkButton`.

## Other available helpers

The package includes these Razor elements:

| Element | Purpose |
| --- | --- |
| `alert` | Alert message with optional title, icon, close button, or child content |
| `breadcrumb` | Breadcrumb from a list of `BreadcrumbItem` values |
| `card` | Panel/card with Razor child content |
| `collapse-panel` | Collapsible panel with a required title and child content |
| `icon` | Font Awesome icon with optional color, tooltip, and class |
| `link-button` | Link, button, or submit control with MVC route support |
| `message-toast` | Toast with title, message, icon, and open state |
| `offcanvas` | Offcanvas region with child content, title, position, and size |
| `validation-summary` | ModelState errors or an explicit error sequence |
| `jj-title` | Heading with subtitle, icon, and action links |

## Breadcrumbs

The `breadcrumb` helper takes an `items` collection of `BreadcrumbItem` values. Build that collection in the page model or view component, then pass it to the view:

```csharp
using JJConsulting.Html.Bootstrap.Models;

public List<BreadcrumbItem> BreadcrumbItems { get; } =
[
    new("Home", "/"),
    new("Customers", "/customers"),
    new("Details")
];
```

```razor
<breadcrumb items="Model.BreadcrumbItems" />
```

The final item is rendered as the current page, without a link.

## Validation summary

`validation-summary` reads errors from the current `ViewContext.ModelState` when no explicit `errors` are supplied. It suppresses itself when there are no errors. You can also pass an explicit sequence:

```razor
<validation-summary title="Please review these issues" />
```

```razor
<validation-summary errors="Model.ImportErrors" title="Import failed" />
```

## Child content and helper behavior

Helpers that support child content retrieve it through Razor's `GetChildContentAsync()` and hand it to the matching component. This applies to `alert`, `card`, `collapse-panel`, and `offcanvas`. Helpers such as `link-button`, `icon`, `breadcrumb`, and `jj-title` build their output from attributes and do not use arbitrary child markup.

Tag Helper output is `IHtmlContent` from the component library, so it is emitted as markup without converting it to an intermediate string. For programmatic composition outside Razor, use the corresponding component classes directly; see [Bootstrap components](components.md).
