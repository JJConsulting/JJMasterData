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
        /// Set a custom Bootstrap data attribute to HTML element.
        /// </summary>
        public static HtmlElement WithDataAttribute(this HtmlElement html, string name, string value)
        {
            string attrName = BootstrapHelper.Version >= 5 ? "data-bs-" : "data-";
            attrName += name;
            return html.WithAttribute(attrName, value);
        }
        
        /// <summary>
        /// Append a hidden input to the Element tree.
        /// </summary>
        public static HtmlElement AppendHiddenInput(this HtmlElement html, string name, string value)
        {
            return html.AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("hidden", "hidden");
                input.WithNameAndId(name);
                input.WithValue(value);
            });
        }

        /// <summary>
        /// Append a hidden input to the Element tree.
        /// </summary>
        public static HtmlElement AppendHiddenInput(this HtmlElement html, string id, string name, string value)
        {
            return html.AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("hidden", "hidden");
                input.WithAttribute("id", id);
                input.WithAttribute("name", name);
                input.WithValue(value);
            });
        }

        /// <summary>
        /// Insert tag script with a script text
        /// </summary>
        public static HtmlElement AppendScript(this HtmlElement html, string rawScript)
        {
            var childElement = new HtmlElement(HtmlTag.Script)
                .WithAttribute("type", "text/javascript")
                .AppendText(rawScript);

            html.AppendElement(childElement);

            return html;
        }
    }
}
