using HtmlAgilityPack;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.Test.Web.Components;

public class JJTextBoxTest
{
    public IHttpContext HttpContext { get; }

    public JJTextBoxTest(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }

    [Fact]
    public void ToStringTest()
    {
        var textBox = GetTextBox();
        var html = textBox.GetHtmlBuilder();

        var document = new HtmlDocument();
        document.LoadHtml(html.ToString(true));

        Assert.NotNull(document.GetElementbyId("textBoxId"));
        Assert.Contains(document.DocumentNode.ChildNodes, node => node.HasClass("class1"));
        Assert.Contains(document.DocumentNode.ChildNodes, node => node.HasClass("class2"));
        Assert.Contains(document.DocumentNode.ChildNodes, node => node.GetAttributeValue("value", null) == "My Text");
        Assert.Contains(document.DocumentNode.ChildNodes, node => node.GetAttributeValue("key", null) == "value");
    }

    private JJTextBox GetTextBox()
    {
        return new JJTextBox(HttpContext)
        {
            Name = "textBoxId",
            ToolTip = "textBoxTest",
            Text = "My Text",
            CssClass = "class1 class2",
            Attributes =
            {
                { "key", "value" }
            }
        };
    }
}