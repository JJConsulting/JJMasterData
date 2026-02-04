# Multiple Forms Support

JJMasterData only supports one form per HTML document.
If you need multiple `JJFormView` or `JJDataPanel` instances, you put them in the same form.

If MasterData does not find the id `masterdata-form`, it will be considered the first form in the DOM.

When you need multiple forms in your View alongside JJMasterData, you have two options:
## Using the TagHelper

```html
<master-data-form asp-controller="Form" asp-action="Render" asp-area="MasterData">
    @Model.FormViewHtml
</master-data-form>
```

## Using the id
<form action="/MyAction" id="masterdata-form">
 @Model.FormViewHtml
</form>