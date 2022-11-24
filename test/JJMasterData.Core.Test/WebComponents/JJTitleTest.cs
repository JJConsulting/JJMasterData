using JJMasterData.Core.Html;
using JJMasterData.Core.WebComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.Test.WebComponents
{
    public class JJTitleTest
    {
        [Fact]
        public void RenderTestAttrs()
        {
            var title = new JJTitle("title", "subtitle");
            title.Size = HeadingSize.H1;
            title.CssClass = "cssClass";
            var actual = title.GetHtmlBuilder();

            var h1 = actual.GetChildren(HtmlTag.H1).First();
            Assert.Contains("cssClass", actual.GetAttribute("class"));
            Assert.NotNull(h1);
            Assert.Equal("title", h1.GetRawText());
            

           
        }

    }
}
