using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Tests;

public class HtmlBuilderAttributeExtensionsTests
{
    [Fact]
    public void WithNameAndId_ShouldSetBoth()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId("abc");

        Assert.Equal("<div id=\"abc\" name=\"abc\"></div>", html.ToString());
    }

    [Fact]
    public void WithAttribute_ShouldOverrideExisting()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("a", "1")
            .WithAttribute("a", "2");

        Assert.Equal("<div a=\"2\"></div>", html.ToString());
    }

    [Fact]
    public void WithAttributeIf_ShouldApplyWhenTrue()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributeIf(true, "x", "y");

        Assert.Equal("<div x=\"y\"></div>", html.ToString());
    }

    [Fact]
    public void WithAttributeIf_ShouldNotApplyWhenFalse()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributeIf(false, "x", "y");

        Assert.Equal("<div></div>", html.ToString());
    }

    [Fact]
    public void WithCssClass_ShouldMergeClasses()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("class", "a b")
            .WithCssClass("b c");

        Assert.Equal("<div class=\"a b c\"></div>", html.ToString());
    }

    [Fact]
    public void WithCssClassIf_ShouldApplyConditionally()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClassIf(true, "c");

        Assert.Equal("<div class=\"c\"></div>", html.ToString());
    }

    [Fact]
    public void WithValue_ShouldSetAttribute()
    {
        var html = new HtmlBuilder(HtmlTag.Input)
            .WithValue("123");

        Assert.Equal("<input value=\"123\" />", html.ToString());
    }

    [Fact]
    public void WithHref_ShouldSetHref()
    {
        var html = new HtmlBuilder(HtmlTag.A)
            .WithHref("http://example.com");

        Assert.Equal("<a href=\"http://example.com\"></a>", html.ToString());
    }

    [Fact]
    public void MultipleAttributes_ShouldRenderAll()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("a", "x")
            .WithAttribute("b", "y")
            .WithAttribute("c", "z");

        Assert.Equal("<div a=\"x\" b=\"y\" c=\"z\"></div>", html.ToString());
    }

    [Fact]
    public void MultipleAttributes_WithSpecialChars_ShouldEncode()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("a", "1&2")
            .WithAttribute("b", "<x>")
            .WithAttribute("c", "\"quoted\"")
            .WithAttribute("d", "'single'")
            .WithAttribute("e", "<>&\"'");

        Assert.Equal(
            "<div a=\"1&amp;2\" b=\"&lt;x&gt;\" c=\"&quot;quoted&quot;\" d=\"&#x27;single&#x27;\" e=\"&lt;&gt;&amp;&quot;&#x27;\"></div>",
            html.ToString()
        );
    }

    [Fact]
    public void Attribute_Overriding_ShouldEncodeInFinalValue()
    {
        var html = new HtmlBuilder(HtmlTag.Input)
            .WithAttribute("value", "<x>")
            .WithAttribute("value", "\"y\"");

        Assert.Equal("<input value=\"&quot;y&quot;\" />", html.ToString());
    }

    [Fact]
    public void Attribute_WithNullValue_ShouldNotThrow_AndShouldNotRender()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("data");

        Assert.Equal("<div data></div>", html.ToString());
    }
}