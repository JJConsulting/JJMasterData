using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.Html
{
    public static class HtmlElementExtensions
    {
        /// <summary>
        /// Set custom data attribute to HTML element.
        /// </summary>
        public static HtmlElement WithValue(this HtmlElement html, string @value)
        {
            return html.WithAttribute("value", @value);
        }
        
        /// <summary>
        /// Set custom data attribute to HTML element.
        /// </summary>
        public static HtmlElement WithHref(this HtmlElement html, string @value)
        {
            return html.WithAttribute("href", @value);
        }
        
        /// <summary>
        /// Set custom data attribute to HTML element.
        /// </summary>
        public static HtmlElement WithDataAttribute(this HtmlElement html, string name, string value)
        {
            return html.WithAttribute($"data-{name}", value);
        }
    }
}
