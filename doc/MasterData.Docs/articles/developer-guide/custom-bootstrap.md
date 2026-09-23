# Custom Bootstrap

Use `CustomBootstrapPath` when the host application needs to replace the Bootstrap version provided by JJMasterData. This property replaces the entire Bootstrap stylesheet, so the configured CSS file must contain a complete, compatible Bootstrap build rather than only custom overrides.

The example below builds Bootstrap 5.3 with a purple `primary` color (`#7c3aed`) as part of the project build. It follows the ASP.NET Core example from [EmbeddedSass.Net](https://github.com/gumbarros/EmbeddedSass.Net/tree/main/samples/EmbeddedSass.Net.Sample.AspNetCore), which compiles Sass through MSBuild without requiring a separate Sass installation.

## 1. Add the Sass compiler and Bootstrap sources

In the ASP.NET Core project hosting JJMasterData, add the `EmbeddedSass.Net.MsBuild` package.

You also need the Bootstrap 5.3 Sass sources. In this example, `Sass/Bootstrap` contains the `scss` directory from the Bootstrap distribution, including `bootstrap.scss` and its partial files.

```xml
<ItemGroup>
  <PackageReference Include="EmbeddedSass.Net.MsBuild"
                    Version="1.2.0"
                    PrivateAssets="all" />
</ItemGroup>

<ItemGroup>
  <EmbeddedSass Include="Sass/masterdata-bootstrap.scss">
    <OutputPath>wwwroot/css/masterdata-bootstrap.css</OutputPath>
    <LoadPaths>Sass/Bootstrap</LoadPaths>
  </EmbeddedSass>
</ItemGroup>
```

> [!NOTE]
> Keep the Sass sources on Bootstrap 5.3.x. JJMasterData relies on classes and components from this version. Test the application before using a compiled stylesheet from a newer major Bootstrap version.

`LoadPaths` allows `@import "bootstrap"` to resolve `Sass/Bootstrap/bootstrap.scss`.

If you prefer to get the sources from NuGet, the `bootstrap.sass` package also includes Bootstrap's Sass files. Copy or expose the directory containing `bootstrap.scss` as a physical `LoadPaths` entry in the project.

## 2. Define the Sass theme

Create `Sass/_theme.scss` to keep the application colors in one place:

```scss
$primary: #7c3aed;
$surface: #f7f3ff;
$body-color: #241b35;
```

Then create `Sass/masterdata-bootstrap.scss`.

Bootstrap variables must be set before importing Bootstrap. This ensures components such as buttons, links, focus states, utilities, and generated CSS variables use the customized values.

```scss
@use "theme";

$primary: theme.$primary;
$body-bg: theme.$surface;
$body-color: theme.$body-color;
$border-radius: 0.75rem;

@import "bootstrap";
```

After running `dotnet build`, `EmbeddedSass.Net.MsBuild` generates:

```text
wwwroot/css/masterdata-bootstrap.css
```

Debug builds generate expanded CSS and a source map. Other build configurations generate compressed CSS by default.

## 3. Configure JJMasterData to use the generated CSS

After calling `AddJJMasterDataWeb`, configure the public path of the generated stylesheet.

The path must start with `/` and point to a file served by the host application's static file middleware.

```csharp
using JJMasterData.Web.Configuration.Options;

builder.Services.AddJJMasterDataWeb(builder.Configuration);

builder.Services.PostConfigure<MasterDataWebOptions>(options =>
{
    options.CustomBootstrapPath = "/css/masterdata-bootstrap.css";
});
```

The application layout should continue rendering the JJMasterData partials:

```html
<head>
    <partial name="_MasterDataStylesheets" />
</head>
<body>
    @RenderBody()
    <partial name="_MasterDataScripts" />
</body>
```

`_MasterDataStylesheets` will reference `masterdata-bootstrap.css` instead of the Bootstrap stylesheet bundled with JJMasterData.

The remaining JJMasterData styles are still included, such as Font Awesome, Flatpickr, Dropzone, and `jjmasterdata.css`.

## Validation

Run:

```bash
dotnet build
```

Then open a JJMasterData page and verify that elements such as `.btn-primary`, links, pagination, active form controls, and `.bg-primary` use `#7c3aed`.

In the browser's Network tab, confirm that `masterdata-bootstrap.css` is the only Bootstrap stylesheet being loaded.

Do not load the default `bootstrap.min.css` at the same time. If both stylesheets are present, the stylesheet loaded last can override the customized Bootstrap rules.

For changes that do not require recompiling Bootstrap, such as spacing or styles for application-specific components, use `CustomStylesheetsPaths` instead.

See also [UI customization](ui-customization.md).
