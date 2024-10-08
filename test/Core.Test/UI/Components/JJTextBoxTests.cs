﻿using System.Text;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using Moq;

namespace JJMasterData.Core.Test.UI.Components;

public class JJTextBoxTests
{
    [Fact]
    public async Task RenderTest()
    {
        var text = GetTextBox();
        var expected = new StringBuilder();
        expected.AppendLine().Append(' ', 2);
        expected.Append("<input ");
        expected.Append("id=\"id1\" ");
        expected.Append("name=\"id1\" ");
        expected.Append("pan=\"pan\" ");
        expected.Append("placeholder=\"00\" ");
        expected.Append("type=\"text\" ");
        expected.Append("class=\"form-control class1 class2\" ");
        expected.Append("title=\"teste\" ");
        expected.Append("data-bs-toggle=\"tooltip\" ");
        expected.Append("value=\"1188880000\" ");
        expected.Append("/>");

        var html = await text.GetHtmlBuilderAsync();
        Assert.Equal(expected.ToString(), html.ToString(true));
    }

    [Fact]
    public async Task RenderTestAttrs()
    {
        var text = GetTextBox();
        var actual = await text.GetHtmlBuilderAsync();

        Assert.Equal(text.Name, actual.GetAttribute("name"));
        Assert.Equal(text.Name, actual.GetAttribute("id"));
        Assert.Equal("pan", actual.GetAttribute("pan"));
        Assert.Equal("00", actual.GetAttribute("placeholder"));
        Assert.Equal("text", actual.GetAttribute("type"));
        Assert.Equal("form-control class1 class2", actual.GetAttribute("class"));
        Assert.Equal("teste", actual.GetAttribute("title"));
        Assert.Equal("tooltip", actual.GetAttribute("data-bs-toggle"));
        Assert.Equal("1188880000", actual.GetAttribute("value"));
    }


    private static JJTextBox GetTextBox()
    {
        return new JJTextBox(new Mock<IFormValues>().Object)
        {
            Name = "id1",
            Tooltip = "teste",
            Text = "1188880000",
            CssClass = "class1 class2",
            PlaceHolder = "00",
            Attributes =
            {
                { "pan", "pan" }
            }
        };
    }
}