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
        public void TestSerialization()
        {
            var text = new JJTextBox();
            text.Name = "id1";
            text.ToolTip = "teste";
            text.Text = "1188880000";
            text.CssClass = "class1 class2";
            text.PlaceHolder = "00";
            text.SetAttr("pan", "pan");

            string html = text.GetHtml();
            const string expected = "<input id=\"id1\" name=\"id1\" type=\"text\" class=\"form-control class1 class2\" value =\"11982941815\" data-bs-toggle=\"tooltip\" title=\"teste\" pan=\"pan\" placeholder=\"00\"/>";
            Assert.Equal(expected, html);
        }


        [Fact]
        public void TestWithActions()
        {
            var text = new JJTextBox();
            text.Name = "id1";
            text.ToolTip = "teste";
            text.Text = "1188880000";
            text.CssClass = "class1 class2";
            text.PlaceHolder = "00";
            text.SetAttr("pan", "pan");

            var action = new JJLinkButton();
            action.Text = "teste";
            action.IconClass = "fa-info";

            text.Actions.Add(action);

            string html = text.GetHtml();
            const string expected = "<input id=\"id1\" name=\"id1\" type=\"text\" class=\"form-control class1 class2\" value =\"11982941815\" data-bs-toggle=\"tooltip\" title=\"teste\" pan=\"pan\" placeholder=\"00\"/>";
            Assert.Equal(expected, html);
        }

    }
}
