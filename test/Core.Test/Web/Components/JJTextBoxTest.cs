using System.Text;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Test.Web.Components
{
    public class JJTextBoxTest
    {
        [Fact]
        public void RenderTest()
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

            var html = text.GetHtmlBuilder();
            Assert.Equal(expected.ToString(), html.ToString(true));
        }

        [Fact]
        public void RenderTestAttrs()
        {
            var text = GetTextBox();
            var actual = text.GetHtmlBuilder();

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


        private JJTextBox GetTextBox()
        {
            return new JJTextBox
            {
                Name = "id1",
                ToolTip = "teste",
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
}