using JJMasterData.Core.Html;
using JJMasterData.Core.WebComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.Test.WebComponents
{
    public class JJTextBoxTest
    {
        [Fact]
        public void RenderTest()
        {
            var text = new JJTextBox();
            text.Name = "id1";
            text.ToolTip = "teste";
            text.Text = "1188880000";
            text.CssClass = "class1 class2";
            text.PlaceHolder = "00";
            text.SetAttr("pan", "pan");

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


    }
}
