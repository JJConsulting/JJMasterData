# TagHelpers

TagHelpers are a simple way to re-use HTML components. JJMasterData have some built-in TagHelpers.

## Usage
At `_ViewImports.cshtml`
```csharp
@addTagHelper *, JJMasterData.Web
```
At your Razor page
```html
<jj-formview element-name="ElementName"/>
```

You can check all available TagHelpers at our [API Reference](https://portal.jjconsulting.com.br/jjdoc/lib/JJMasterData.Web.TagHelpers.html)
