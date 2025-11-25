#nullable enable

using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;


namespace JJMasterData.Core.UI.Components;

public class JJMessageBox : HtmlComponent
{
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public MessageIcon Icon { get; set; }
    public MessageSize Size { get; set; }
    
    public string? Button1Label { get; set; }
    public string? Button1JsCallback { get; set; }
    
    public string? Button2Label { get; set; }
    public string? Button2JsCallback { get; set; }
    
    internal JJMessageBox()
    {
    }

    internal override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Script)
            .WithAttribute("type", "text/javascript")
            .WithAttribute("lang", "javascript")
            .AppendText(GetShowScript());

        return html;
    }

    public string GetDomContentLoadedScript()
    {
        var showScript = GetShowScript();
        return $$"""
                 document.addEventListener('DOMContentLoaded', function() {
                        {{showScript}}
                    });
                 """;
    }

    public string GetShowScript()
    {
        var title = HttpUtility.JavaScriptStringEncode(Title);
        var message = HttpUtility.JavaScriptStringEncode(Content ?? string.Empty);

        var script = $"""
                      MessageBox.show(
                                         '{title}',
                                         '{message}',
                                         '{(int)Icon}',
                                         '{(int)Size}'
                      """;

        if (!string.IsNullOrEmpty(Button1JsCallback))
        {
            script += $", '{Button1Label}', ()=>{{{Button1JsCallback};}}";
        }

        if (!string.IsNullOrEmpty(Button2JsCallback))
        {
            script += $", '{Button2Label}', ()=>{{{Button2JsCallback}}}";
        }

        script += ");";
    
        return script;
    }
}