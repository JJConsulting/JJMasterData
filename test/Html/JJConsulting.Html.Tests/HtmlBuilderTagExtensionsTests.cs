using static JJConsulting.Html.Extensions.HtmlBuilderTagExtensions;

namespace JJConsulting.Html.Tests;

public class HtmlBuilderTagExtensionsTests
{
    [Fact]
    public void Div_WithChildren_RendersCorrectHtml()
    {
        var builder = Div(
            H1("Hello World"),
            P("Lorem ipsum")
        );

        var html = builder.ToString();

        Assert.Contains("<div>", html);
        Assert.Contains("<h1>Hello World</h1>", html);
        Assert.Contains("<p>Lorem ipsum</p>", html);
        Assert.EndsWith("</div>", html);
    }

    [Fact]
    public void Div_WithRawText_RendersContent()
    {
        var builder = Div("Sample");
        var html = builder.ToString();

        Assert.Equal("<div>Sample</div>", html);
    }

    [Fact]
    public void Div_WithRawText_EncodeTrue_EncodesCorrectly()
    {
        var builder = Div("<b>X</b>");
        var html = builder.ToString();

        Assert.Equal("<div>&lt;b&gt;X&lt;/b&gt;</div>", html);
    }

    [Fact]
    public void H1_RendersHeading()
    {
        var html = H1("Title").ToString();
        Assert.Equal("<h1>Title</h1>", html);
    }

    [Fact]
    public void NestedTags_RenderInCorrectOrder()
    {
        var builder = Div(
            Div(
                P("Inner")
            )
        );

        var html = builder.ToString();

        Assert.Equal("<div><div><p>Inner</p></div></div>", html);
    }

    [Fact]
    public void MultipleChildren_RenderSequentially()
    {
        var builder = Div(
            Span("A"),
            Span("B"),
            Span("C")
        );

        var html = builder.ToString();

        Assert.Equal("<div><span>A</span><span>B</span><span>C</span></div>", html);
    }

    [Fact]
    public void DeepStructure_RendersCompleteTree()
    {
        var builder = Div(
            Header(
                H1("Top"),
                P("Subtitle")
            ),
            Main(
                Article(
                    H2("Heading"),
                    P("Text")
                )
            ),
            Footer("Bottom")
        );

        var html = builder.ToString();

        Assert.Contains("<header><h1>Top</h1><p>Subtitle</p></header>", html);
        Assert.Contains("<main><article><h2>Heading</h2><p>Text</p></article></main>", html);
        Assert.Contains("<footer>Bottom</footer>", html);
        Assert.StartsWith("<div>", html);
        Assert.EndsWith("</div>", html);
    }

    [Fact]
    public void EmptyChildrenList_RendersEmptyTag()
    {
        var builder = Div();
        var html = builder.ToString();

        Assert.Equal("<div></div>", html);
    }

    [Fact]
    public void ListItemStructure_RendersCorrectUl()
    {
        var builder = Ul(
            Li("A"),
            Li("B"),
            Li("C")
        );

        var html = builder.ToString();

        Assert.Equal(
            "<ul><li>A</li><li>B</li><li>C</li></ul>",
            html
        );
    }
}