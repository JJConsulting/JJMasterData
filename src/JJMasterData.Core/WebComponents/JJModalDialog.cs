using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Language;

namespace JJMasterData.Core.WebComponents;

public class JJModalDialog : JJBaseView
{
    public string Title { get; set; }

    public string HtmlContent { get; set; }

    public List<JJLinkButton> Buttons { get; set; }

    public MessageSize Size { get; set; }

    public JJModalDialog()
    {
        Name = "jjmodal";
        Size = MessageSize.Large;
        Buttons = new List<JJLinkButton>();
    }

    protected override string RenderHtml()
    {
        string title = Translate.Key(Title);

        const char tab = '\t';
        StringBuilder html = new();
        html.Append(tab, 1);
        html.AppendLine($"<div class=\"modal {CssClass}\" id=\"{Name}\" role=\"dialog\"  aria-labelledby=\"{Name}-label\" aria-hidden=\"true\"> ");
        html.Append(tab, 2);
        html.Append("<div class=\"modal-dialog ");
        html.Append(GetSizeClass());
        html.AppendLine("\"> ");
        html.Append(tab, 3);
        html.AppendLine("<div class=\"modal-content\"> ");
        html.Append(tab, 4);
        html.AppendLine("<div class=\"modal-header\"> ");
        html.Append(tab, 5);
        html.AppendLine($"<div id=\"{Name}-label\" class=\"modal-title\">{title}</h4> ");
        if(BootstrapHelper.Version == 3)
        {
            html.AppendLine($"<button type=\"button\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"modal\">{BootstrapHelper.CloseButtonTimes}</button> ");
        }
        html.Append(tab, 4);
        html.Append(tab, 5);
        html.AppendLine("</div> ");
        if(BootstrapHelper.Version > 3)
        {
            html.AppendLine($"<button type=\"button\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"modal\">{BootstrapHelper.CloseButtonTimes}</button> ");
        }
        html.AppendLine("</div> ");
        html.Append(tab, 4);
        html.AppendLine("<div class=\"modal-body\"> ");
        html.AppendLine(HtmlContent);
        html.Append(tab, 4);
        html.AppendLine("</div> ");

        if (Buttons.Count > 0)
        {
            html.Append(tab, 4);
            html.AppendLine("<div class=\"modal-footer\"> ");
            foreach (var btn in Buttons)
            {
                html.Append(tab, 5);
                html.AppendLine(btn.GetHtml());
            }
            html.Append(tab, 4);
            html.AppendLine("</div> ");
        }
        
        html.Append(tab, 3);
        html.AppendLine("</div> ");
        html.Append(tab, 2);
        html.AppendLine("</div> ");
        html.Append(tab, 1);
        html.AppendLine("</div>");

        return html.ToString();
    }


    private string GetSizeClass() => 
        Size switch
        {
            MessageSize.Small => "modal-sm",
            MessageSize.Default => "modal-md",
            _ => "modal-lg",
        };
}
