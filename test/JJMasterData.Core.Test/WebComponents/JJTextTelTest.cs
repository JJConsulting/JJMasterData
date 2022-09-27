using JJMasterData.Core.WebComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.Test.WebComponents
{
    public class JJTextTelTest
    {
        [Fact]
        public void TestSerialization()
        {
            var text = new JJTextTel();
            text.Name = "id1";
            text.ToolTip = "teste";
            text.Text = "11982941815";
            text.CssClass = "class1 class2";
            text.PlaceHolder = "00";
            text.SetAttr("pan", "pan");

            string html = text.GetHtml();
            const string expected = "<div class=\"input-group\"><div class=\"input-group-text\"><span class=\"fa fa-phone\"></span><span title=\"Brasil\">&nbsp;&nbsp;+55</span></div><input id=\"id1\" name=\"id1\" title=\"teste\" data-bs-toggle=\"tooltip\" pan=\"pan\" placeholder=\"00\" class=\"class1 class2 form-control\" type=\"tel\" maxlength=\"19\" data-inputmask=\"'mask': '[(99) 99999-9999]', 'placeholder':'', 'greedy': 'false'\" value=\"11982941815\" disabled=\"disabled\"/></div>";
            Assert.Equal(expected, html);
        }
    }
}
