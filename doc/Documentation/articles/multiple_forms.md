# Using multiple forms alongside JJMasterData

When you need multiple forms in your View alongside JJMasterData, you have two options:

## Using the TagHelper

```html
<master-data-form asp-controller="Form" asp-action="Render" asp-area="MasterData">
    @Html.Raw(Model.FormViewHtml)
</master-data-form>
```

## Using the id
<form action="/MyAction" id="masterdata-form">
 @Html.Raw(Model.FormViewHtml)
</form>