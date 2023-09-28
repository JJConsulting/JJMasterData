using System.Web;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJMessageBox : HtmlComponent
{
    private string _text;

    public string Text
    {
        get => _text;

        set => _text = !string.IsNullOrEmpty(value) ? value.Replace("'", "`") : value;
    }

    public string Title { get; set; }
    public MessageIcon Icon { get; set; }
    public MessageSize Size { get; set; }
    
    public string Button1Label { get; set; }
    public string Button1JsCallback { get; set; }
    
    public string Button2Label { get; set; }
    public string Button2JsCallback { get; set; }
    
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

        var message = Text.Replace("<br>", "\\n").Replace("\r\n", string.Empty);
        return $$"""
                 document.addEventListener('DOMContentLoaded', function() {
                        MessageBox.show(
                        '{{Title}}',
                        '{{message}}',
                        '{{(int)Icon}}',
                        '{{(int)Size}}',
                        '{{Button1Label}}',
                        {{Button1JsCallback}},
                        '{{Button2Label}}',
                        {{Button2JsCallback}})
                    });
                 """;
    }
}