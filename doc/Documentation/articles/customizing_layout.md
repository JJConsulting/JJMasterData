# Customizing Layout

By default a bootstrap.min.css is added, but you can modified it.<br>

You have two options.

## Using your own Bootstrap

Setting the `CustomBootstrapPath` property, `bootstrap.min.css` not will be generate in `_MasterDataStylesheets` and your custom path will be used.
```cs
builder.Services.AddJJMasterDataWeb(options =>
{
    options.ConfigureWeb = webOptions =>
    {
        webOptions.CustomBootstrapPath = "/home/gumbarros/css/bootstrap.css";
    };
})
```
> [!TIP]
> You can also set this variable in a `appsettings.json`, see how to do this in [configurations](configurations.md) section.

## Using your own Layout

To use your own the layout, modify the properties `LayoutPath` and `ModalLayoutPath` at <xref:JJMasterData.Web.Configuration.Options.MasterDataWebOptions>

Also don't forget to add our partials to have the required front-end dependencies.

Your custom layout will look like this:
```html
@using Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, JJMasterData.Web
<!DOCTYPE html>
<html data-bs-theme="light" lang="@System.Globalization.CultureInfo.CurrentCulture.Name">
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"]</title>

    <!-- do not remove -->
    <partial name="_MasterDataStylesheets"/>
    @await RenderSectionAsync("Stylesheets", required: false)
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