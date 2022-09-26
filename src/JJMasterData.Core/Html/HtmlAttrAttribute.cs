using System;

namespace JJMasterData.Core.Html
{
    internal class HtmlAttrAttribute : Attribute
    {
        private string v;

        public HtmlAttrAttribute(string v)
        {
            this.v = v;
        }
    }
}