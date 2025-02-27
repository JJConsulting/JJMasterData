using System.Text;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.Test.UI.Html;

public class HtmlBuilderTests
{
    [Theory]
    [InlineData(HtmlTag.Div)]
    [InlineData(HtmlTag.Span)]
    public void RenderTagWithCloseTag_Test(HtmlTag tag)
    {
        var builder = new HtmlBuilder();
        builder.Append(tag);
        var result = builder.ToString();

        string formattedTag = tag.ToString().ToLower();
        Assert.Equal($"<{formattedTag}></{formattedTag}>", result);
    }

    [Theory]
    [InlineData(HtmlTag.Br)]
    public void RenderTagSelfClosed_Test(HtmlTag tag)
    {
        var builder = new HtmlBuilder(tag);
        var result = builder.ToString();

        string formattedTag = tag.ToString().ToLower();
        Assert.Equal($"<{formattedTag} />", result);
    }

    [Fact]
    public void RenderElementText_Test()
    {
        var builder = new HtmlBuilder(HtmlTag.Span)
            .AppendText("test");

        Assert.Equal("<span>test</span>", builder.ToString());

    }

    [Fact]
    public void RenderElementIndentation_Test()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
             .AppendText("test2")
             .Append(HtmlTag.Span, s =>
             {
                 s.AppendText("test1");
             });

        var shtml = new StringBuilder();
        shtml.AppendLine().Append(' ', 2);
        shtml.Append("<div>");
        shtml.AppendLine().Append(' ', 4);
        shtml.Append("test2");
        shtml.AppendLine().Append(' ', 4);
        shtml.Append("<span>");
        shtml.AppendLine().Append(' ', 6);
        shtml.Append("test1");
        shtml.AppendLine().Append(' ', 4);
        shtml.Append("</span>");
        shtml.AppendLine().Append(' ', 2);
        shtml.Append("</div>");
        Assert.Equal(shtml.ToString(), builder.ToString(true));
    }

    [Fact]
    public void RenderComplex_Test()
    {
        var builder = new HtmlBuilder(HtmlTag.Div)
            .WithNameAndId("id1")
            .WithCssClass("class1, class2")
            .Append(HtmlTag.H1, h1 =>
            {
                h1.AppendText("Simple Title"); 
                h1.Append(HtmlTag.Small, s =>
                {
                    s.AppendText("This is a subtitle");
                });
            });

        var shtml = builder.ToString(indentHtml: true);
        
        Assert.Equal(shtml, builder.ToString(true));
    }
    
    
}