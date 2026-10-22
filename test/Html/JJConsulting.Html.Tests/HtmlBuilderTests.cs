namespace JJConsulting.Html.Tests;

public class HtmlBuilderTests
{
    [Fact]
    public void EmptyBuilder_ShouldReturnEmptyString()
    {
        var builder = new HtmlBuilder();
        var result = builder.ToString();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void RawText_ShouldRenderAsIs()
    {
        var builder = new HtmlBuilder("hello");
        var result = builder.ToString();
        Assert.Equal("hello", result);
    }

    [Fact]
    public void RawText_ShouldEncode()
    {
        var builder = new HtmlBuilder("<h1>Hello</h1>");
        var result = builder.ToString();
        Assert.Equal("&lt;h1&gt;Hello&lt;/h1&gt;", result);
    }

    [Fact]
    public void TagWithoutClosing_ShouldRenderSelfClosing()
    {
        var builder = new HtmlBuilder(HtmlTag.Img)
            .WithAttribute("src", "test.png");
        var result = builder.ToString();
        Assert.Equal("<img src=\"test.png\" />", result);
    }

    [Fact]
    public void TagWithClosing_ShouldRenderContent()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .Append(new HtmlBuilder("text"));
        var result = builder.ToString();
        Assert.Equal("<div>text</div>", result);
    }

    [Fact]
    public void Attributes_ShouldEncodeValues()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("data-test", "a \"quoted\" value & more");
        var result = builder.ToString();
        Assert.Equal("<div data-test=\"a &quot;quoted&quot; value &amp; more\"></div>", result);
    }

    [Fact]
    public void MultipleAttributes_ShouldRenderAll()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("a", "1")
            .WithAttribute("b", "2");
        var result = builder.ToString();
        Assert.Equal("<div a=\"1\" b=\"2\"></div>", result);
    }

    [Fact]
    public void Children_ShouldRenderInOrder()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .Append(new HtmlBuilder("child1"))
            .Append(new HtmlBuilder("child2"));
        var result = builder.ToString();
        Assert.Equal("<div>child1child2</div>", result);
    }

    [Fact]
    public void Prepend_ShouldInsertAtStart()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .Append(new HtmlBuilder("b"))
            .Prepend(new HtmlBuilder("a"));
        var result = builder.ToString();
        Assert.Equal("<div>a b".Replace(" ", "") + "</div>", result.Replace(" ", ""));
    }

    [Fact]
    public void AppendRange_ShouldAppendAll()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .AppendRange([
                new HtmlBuilder("x"),
                new HtmlBuilder("y"),
                new HtmlBuilder("z")
            ]);
        var result = builder.ToString();
        Assert.Equal("<div>xyz</div>", result);
    }

    [Fact]
    public void TextArea_ShouldNotIndentContent()
    {
        var builder = new HtmlBuilder(HtmlTag.TextArea)
            .Append(new HtmlBuilder("line1\nline2"));
        var result = builder.ToString();
        Assert.Equal("<textarea>line1&#xA;line2</textarea>", result);
    }

    [Fact]
    public void GetAttribute_ShouldReturnValue()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("a", "b");
        var value = builder.GetAttribute("a");
        Assert.Equal("b", value);
    }

    [Fact]
    public void TryGetAttribute_ShouldReturnTrueWhenExists()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("a", "b");
        var success = builder.TryGetAttribute("a", out var value);
        Assert.True(success);
        Assert.Equal("b", value);
    }

    [Fact]
    public void Append_SelfReference_ShouldThrow()
    {
        var builder = new HtmlBuilder(HtmlTag.Div);
        Assert.Throws<InvalidOperationException>(() => builder.Append(builder));
    }

    [Fact]
    public void Prepend_SelfReference_ShouldThrow()
    {
        var builder = new HtmlBuilder(HtmlTag.Div);
        Assert.Throws<InvalidOperationException>(() => builder.Prepend(builder));
    }
}
