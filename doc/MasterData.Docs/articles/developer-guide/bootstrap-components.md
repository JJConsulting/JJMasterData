# Bootstrap Components (`JJConsulting.Html.Bootstrap`)

`JJConsulting.Html.Bootstrap` provides reusable Bootstrap-aligned HTML components for server-rendered UI. Components are C# objects that render to `HtmlBuilder` and ASP.NET Core `IHtmlContent`, so they can be used in MVC, Razor, Tag Helpers, and code that already uses `JJConsulting.Html`.

Install the package:

```bash
dotnet add package JJConsulting.Html.Bootstrap
```

The host application supplies the Bootstrap CSS and JavaScript assets. This package generates markup and compatibility classes; it does not add Bootstrap assets to the page. Load a matching Bootstrap distribution and include its JavaScript bundle when using interactive components such as modals, offcanvas panels, collapse panels, or toasts. See [Custom Bootstrap](custom-bootstrap.md) for stylesheet configuration in JJMasterData.

Import the component and model namespaces:

```csharp
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
```

## Render a component

Components derive from `HtmlComponent`, which implements `IHtmlContent`. Use `GetHtmlBuilder()` when composing markup, `GetHtml()` when a string is explicitly needed, or append the component directly to a builder with `AppendComponent`.

```csharp
var alert = new JJAlert
{
    Title = "Saved",
    Color = BootstrapColor.Success
};
alert.Messages.Add("The customer record was saved.");

var page = HtmlBuilder.Div()
    .WithCssClass("container")
    .AppendComponent(alert);
```

`Visible` defaults to `true`. A component with `Visible = false` renders no content. Set `Name`, `CssClass`, and `Attributes` on a component to configure its outer element where supported.

## Common components

### Alerts

`JJAlert` renders a Bootstrap alert with optional title, icon, close button, content, and messages.

```csharp
var alert = new JJAlert
{
    Title = "Review required",
    Color = BootstrapColor.Warning,
    ShowCloseButton = true
};
alert.Messages.Add("Check the billing address before continuing.");
```

Use `Content` for an `IHtmlContent` fragment when the alert body needs composed markup. `Messages` are rendered as raw HTML by the current implementation; only put trusted, application-controlled strings in that collection. For user-provided text, compose an encoded `HtmlBuilder` fragment and assign it to `Content`.

### Cards and panels

`JJCard` accepts an `IHtmlContent` body, an optional title and subtitle, and a `PanelLayout`. The default layout is `Panel`.

```csharp
var card = new JJCard
{
    Title = "Customer",
    SubTitle = "Account details",
    Layout = PanelLayout.Panel,
    Content = HtmlBuilder.P().AppendText("Active account")
};

var markup = card.GetHtml();
```

`PanelLayout.Well` uses a well or card-body style, depending on the configured Bootstrap version. `PanelLayout.NoDecoration` renders the content without the panel wrapper styling.

### Titles and actions

`JJTitle` renders a page heading with an optional subtitle, icon, size, and action links.

```csharp
var title = new JJTitle("Customers", "Manage customer accounts")
{
    Size = HeadingSize.H1,
    Actions =
    [
        new TitleAction { Text = "Add customer", Url = "/customers/new" }
    ]
};
```

### Link buttons

`JJLinkButton` can render a link, button, or submit control. Set `UrlAction`, `Type`, `Text`, `IconClass`, `Color`, `Enabled`, or `ShowAsButton` to match the interaction. `InnerHtml` can hold trusted composed markup in addition to the text and icon.

```csharp
var save = new JJLinkButton
{
    Text = "Save",
    Type = LinkButtonType.Submit,
    ShowAsButton = true,
    Color = BootstrapColor.Primary
};
```

### Other components

The package also includes `JJBreadcrumb`, `JJIcon`, `JJImage`, `JJLabel`, `JJMessageBox`, `JJMessageToast`, `JJModalDialog`, `JJOffcanvas`, `JJSpinner`, `JJTabNav`, `JJText`, `JJToolbar`, and `JJValidationSummary`. Most accept `HtmlComponent` settings and can be appended to a builder with `AppendComponent`.

Use trusted application-controlled values for `JJTitle.Title` and `JJTitle.SubTitle`: the current component writes those values as raw HTML. Prefer encoded text nodes from `HtmlBuilder.AppendText` when the values can contain user input.

## Bootstrap version compatibility

Components default to Bootstrap 5 compatibility. The version is read once from `AppContext` when `BootstrapHelper` is first initialized. Set it early in application startup if the host uses Bootstrap 3 or 4:

```csharp
AppContext.SetData("JJMasterData.BootstrapVersion", 5);
```

Use the same major version for the stylesheets and scripts loaded by the host. Compatibility mode adjusts classes and data attributes, but it does not load or polyfill Bootstrap assets.

## Compose with `HtmlBuilder`

Components are interoperable with `JJConsulting.Html`. Build structure with `HtmlBuilder`, then append components where convenient:

```csharp
var content = HtmlBuilder.Section()
    .WithCssClass("customer-summary")
    .AppendComponent(new JJTitle("Customer", "Account overview"))
    .AppendComponent(new JJAlert
    {
        Color = BootstrapColor.Info,
        Content = HtmlBuilder.P().AppendText("Details are current.")
    });
```

See [HtmlBuilder](html-builder.md) for text encoding, attributes, and composition behavior.
