using JJMasterData.Core.WebComponents;
using HtmlAgilityPack;

namespace JJMasterData.Core.Test.WebComponents;

public class JJTextBoxTest
{
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

    private static JJTextBox GetTextBox()
    {
        return new JJTextBox()
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