# TagHelpers

TagHelpers are a simple way to re-use HTML components. <br>
JJMasterData have some built-in TagHelpers.<br>
<br>
The big advantage is to encapsulate the "bootstrap" components, 
so the html code is generated according to the parameterized version of bootstrap.
With this we hope to eliminate the pain of updating bootstrap versions.

## Usage
At `_ViewImports.cshtml`
```csharp
@addTagHelper *, JJMasterData.Web
```
## Examples

### Title
```html
 <jj-title title="My title" subtitle="Foo"/>
```
In the example above the following HTML code is generated
```html
<h1>
    My Title
    <small>Foo</small>
</h1>
```

> [!TIP]
> Title is the only with the prefix jj because of a conflict with the title HTML tag.

### CollapsePanel
```html
<collapse-panel model="Model" icon="Check" title="Dynamic Collapse Panel" partial="_MyPartialView" expanded-by-default="true"/>

<collapse-panel  icon="Table" title="Static Collapse Panel"  expanded-by-default="true">
    My static collapse-panel
</collapse-panel>
```
<br>

### Tooltip
```html
   <tooltip title="This is a help text inside a ? icon"/>
```

### Checkbox
```html
    <checkbox for="Form.IsVerticalLayout" switch="true"/>
```
### LinkButton
````html
<link-button 
        icon="@Home" 
        url-action="http://www.jjconsulting.tech"
        text="JJ Consulting"/>
````
You can check all available TagHelpers at <xhref:JJMasterData.Web.TagHelpers>
