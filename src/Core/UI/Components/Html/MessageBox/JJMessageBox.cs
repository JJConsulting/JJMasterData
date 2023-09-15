using System.Text;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJMessageBox : HtmlComponent
{
    
    private string _text;
    public string Text
    {
        get => _text;
        
        set => _text = !string.IsNullOrEmpty(value) ? value.Replace("'","`") : value;
    }
    
    public string Title { get; set; }
    public MessageIcon Icon { get; set; }
    public MessageSize Size { get; set; }

    internal JJMessageBox()
    {

    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .WithAttribute("lang", "javascript")
            .AppendText(GetScript());

        return html;
    }

    public string GetScript()
    {
        var javaScript = new StringBuilder();
        
        string msg = Text;
        javaScript.AppendLine("$(document).ready(function() {");
        javaScript.Append("\t\t\t");
        javaScript.Append("messageBox.show('");
        javaScript.Append(Title);
        javaScript.Append("','");
        javaScript.Append(msg.Replace("<br>", "\\r\\n").Replace("\r\n", ""));
        javaScript.Append("', ");
        javaScript.Append((int)Icon);
        javaScript.Append(", ");
        javaScript.Append((int)Size);
        javaScript.AppendLine(");");
        javaScript.Append("\t\t");
        javaScript.AppendLine("});");

        return javaScript.ToString();
    }


}
