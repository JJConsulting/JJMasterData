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
