# Customizing Layout

By default a bootstrap.min.css is added, but you can modified it.<br>

You have two options.

## Using your own layout

Setting `CustomBootstrapPath` to `true` the link from default `bootstrap.min.css` not will be generate in `_MasterDataStylesheets` and your custom path will be used.
```cs
builder.Services.AddJJMasterDataWeb(o =>
{
    o.CustomBootstrapPath = "/home/gumbarros/css/bootstrap.css"
})
```
> [!TIP]
> You can also set this variable in a `appsettings.json`, see how to do this in [configurations](configurations.md) section.

In your project folder Views/Shared add the files:
```
_MasterDataLayout.cshtml
_MasterDataLayout.Modal.cshtml
```
> [!TIP]
> You can modified the layout name, setting the variables `JJMasterDataWeb.LayoutPath` and `JJMasterDataWeb.ModalLayoutPath`

Inside this file put a your custom link with the .css file.<br>
It'll look like this:

```html
@using Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, JJMasterData.Web
<!DOCTYPE html>
<html lang="@System.Globalization.CultureInfo.CurrentCulture.Name">
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>JJMasterData</title>

    <!-- do not remove -->
    <partial name="_MasterDataStylesheets"/>
    <partial name="_MasterDataScripts"/>
</head>
<body>

<div class="container-fluid">
    @RenderBody()
</div>

@await RenderSectionAsync("Scripts", required: false)

</body>
</html>
```
At runtime, the system will check if the `_MasterDataLayout.cshtml` & `_MasterDataLayout.PopUp.cshtml` (or your custom layouts path) exist, if so, it will use the layouts configured in that files.

#