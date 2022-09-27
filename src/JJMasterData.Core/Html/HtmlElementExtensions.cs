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
        // public static HtmlElement WithStyle(this HtmlElement element, string style)
        // {
        //     element.WithAttribute("style", style);
        //     return element;
        // }
        //
        // public static HtmlElement WithStyle(this HtmlElement element, Dictionary<string,string> styles)
        // {
        //     string styleString = string.Join(";", styles.Select(x => x.Key + ":" + x.Value).ToArray());
        //     element.WithAttribute("style", styleString);
        //     return element;
        // }
        //
        // public static HtmlElement AppendIcon(this HtmlElement element, IconType icon)
        // {
        //     element.AppendText(new JJIcon(icon).GetHtmlElement());
        //     return element;
        // }
    }
}
