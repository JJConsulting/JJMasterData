# HtmlBuilder Class
Light way for creating HTML string with fluent method syntax of C#.<br>
All Components generate a HtmlBuilder.

[HtmlBuilder Class](https://portal.jjconsulting.com.br/jjdoc/lib/JJMasterData.Core.Web.Html.HtmlBuilder.html)

## Examples
Single element
```csharp
var builder = new HtmlBuilder(HtmlTags.Input)
            .WithAttribute("type", "text")
            .WithAttribute("name", "id1")
            .WithAttribute("placeholder", "Enter a text");

string actualHtml = builder.ToString();

// Builder result
// <input type="text" name="id1" placeholder="Enter a text" />
```

Complex element
```csharp
var builder = new HtmlBuilder(HtmlTag.Div)
    .WithNameAndId("id1")
    .WithCssClass("class1 class2")
    .AppendElement(HtmlTag.H1, h1 =>
    {
        h1.AppendText("Simple Title"); 
        h1.AppendElement(HtmlTag.Small, s =>
        {
            s.AppendText("This is a subtitle");
        });
    });

var actualHtml = builder.ToString(indentHtml: true);

// Builder result
//<div id="id1" name="id1" class="class1 class2">
//  <h1>
//    Simple Title
//    <small>
//      This is a subtitle
//    </small>
//  </h1>
//</div>
```