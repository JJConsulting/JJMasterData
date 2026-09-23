# UI customization

Change dictionary metadata for field layout; change `MasterDataWebOptions` for the host layout and shared assets.

```csharp
var name = form.FormElement.Fields["Name"];
name.PanelId = 1; // Must match an existing panel.
name.LineGroup = 1;
name.CssClass = "col-sm-6";
name.HelpDescription = "Name shown in the customer list";
```

Fields with the same `LineGroup` share a form row. Use `GridWidth`, `GridAlignment` and `GridRenderingTemplate` for grid-specific presentation. `EncodeHtml` defaults to `true`.

In the host layout:

```html
<partial name="_MasterDataStylesheets" />
<partial name="_MasterDataScripts" />
```

Register shared CSS/JS through `CustomStylesheetsPaths` and `CustomScriptsPaths`. `CustomBootstrapPath` replaces the default Bootstrap stylesheet, so the supplied file must provide the Bootstrap styles expected by the components. See [custom Bootstrap 5.3](custom-bootstrap.md) for a Sass compilation example.

For per-dictionary changes from C#, use `OnFormElementLoadAsync`; for reusable Razor components, see [templates and UI](templates-troubleshooting.md#template-and-ui-reference).

## Customize a form instance

After creating a form with `FormViewFactory`, change its metadata before rendering:

```csharp
var formView = await formViewFactory.CreateAsync("Person");
formView.FormElement.Title = "Runtime customization";
```

These changes apply only to that form instance. Use a registered [form event handler](form-events.md) when the customization must apply whenever the dictionary is loaded.
