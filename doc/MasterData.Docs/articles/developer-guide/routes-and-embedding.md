# Routes and embedding

The Web package maps two MVC areas:

| Registration | Route |
| --- | --- |
| `MapDataDictionary()` | `/DataDictionary/{controller=Element}/{action=Index}/{elementName?}/{fieldName?}` |
| `MapMasterData()` | `/MasterData/{controller}/{action}/{elementName?}/{fieldName?}/{id?}` |

Open a CRUD at `/MasterData/Form/Render/Person`, where `Person` is the dictionary `Name`.

For an existing controller, inject `IFormElementComponentFactory<JJFormView>` from `JJMasterData.Core.UI.Components` and follow the built-in controller's result handling:

```csharp
var form = await formViewFactory.CreateAsync("Person");
var result = await form.GetResultAsync();
if (result is IActionResult actionResult)
    return actionResult;

ViewData["FormViewHtml"] = result.HtmlContent;
return View();
```

`HtmlContent` is an extension from `JJMasterData.Web.Extensions`. Use the MasterData form Tag Helper in the view (register `@addTagHelper *, JJMasterData.Web` in `_ViewImports.cshtml`):

```html
<master-data-form asp-controller="People" asp-action="Index" method="post">
    @ViewData["FormViewHtml"]
</master-data-form>
```

Replace `People/Index` with the custom controller/action that processes the form, supporting both its initial GET and subsequent POST requests. Include the MasterData asset partials in the layout. Preserve action results so AJAX responses, downloads and redirects are not converted into empty HTML.

Prefer `CreateAsync(name)` when using registered form events; the factory attaches the handler for that dictionary.

## Preserve context across requests

Configure the runtime form before `GetResultAsync()` on every request, including POST and AJAX requests. A filter or default added only during the initial GET will not reliably configure later submissions. Give each component a stable, distinct `Name` when several components share a page.

For a child CRUD, apply both the parent's grid filter and the foreign key's default: filtering the list does not populate a new record. Enforce parent ownership during writes and direct-record lookup as well. Keep these runtime changes on the form instance rather than persisting request-specific defaults into shared dictionary metadata.

## Choose the component for a custom screen

Use `JJFormView` when the generated CRUD workflow should process actions and saves. Use a standalone `JJDataPanel` for fields inside a host-owned page, and `JJGridView` for separately placed lists. These components have different factory and persistence behavior; see [Data panels and application-owned saves](data-panels.md).

A composed page can render a grid's toolbar with `GetToolbarHtmlAsync()` and its table with `GetTableHtmlAsync()`. Rendering those fragments does not implement the host's save, add/remove tracking or aggregate transaction. See [Save a composed document](../cookbook/composed-document.md) for a header and multiple editable detail lists.
