using System.Collections;
using System.Text;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJSpinner : JJBaseView
{
    protected override string RenderHtml()
    {
        CssClass += "spinner-grow spinner-grow-sm text-info";
        
        if (BootstrapHelper.Version == 3)
            CssClass += " jj-blink";

        var html = new StringBuilder();
        html.Append("<span role=\"status\" ");
        html.AppendFormat("class=\"{0}\" ", CssClass);
        if (!string.IsNullOrEmpty(Name))
            html.AppendFormat("id=\"{0}\" ", Name);

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(" ");
            html.Append(attr.Key);
            
            if (attr.Value == null) continue;
            
            html.Append("=\"");
            html.Append(attr.Value);
            html.Append("\"");
        }

        html.AppendLine(">");
        if (BootstrapHelper.Version == 3)
        {
            var icon = new JJIcon(IconType.Circle);
            html.AppendLine(icon.GetHtml());
        }
        else
        {
            html.AppendLine("<span class=\"visually-hidden\">Background Process Loading...</span>");
        }
                
        html.AppendLine("</span>");

        return html.ToString();
    }
}