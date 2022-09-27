using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JJMasterData.Core.WebComponents
{
    public class JJAlert : JJBaseView
    {
        public PanelColor Color { get; set; }
        public IconType Icon { get; set; }
        public string Title { get; set; }
        public List<string> Messages { get; set; }
        public bool ShowCloseButton { get; set; }
        private string ClassType
        {
            get
            {
                if (Color.Equals(PanelColor.Default))
                {
                    return "secondary";
                }
                return Color.ToString().ToLower();

            }
        }
        public JJAlert()
        {
            Messages = new List<string>();
        }


        private string GetSplittedMessages() => string.Join("<br>", Messages.Select(m => Translate.Key(m)));

        protected override string RenderHtml()
        {
            var html = new HtmlBuilder();
            if (BootstrapHelper.Version == 3 & (Color == PanelColor.Default || Color == PanelColor.Primary))
            {
                html.StartElement(GetAlertDefaultBs3());
            }
            else
            {
                html.StartElement(GetAlert());
            }

            return html.RenderHtml();
        }

        private HtmlElement GetAlertDefaultBs3()
        {
            var html = new HtmlElement(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithCssClass($"alert {BootstrapHelper.Well}")
            .WithAttributes(Attributes)
            .AppendElement(HtmlTag.A, a =>
            {
                a.WithAttribute("href", "#");
                a.WithAttribute("aria-label", Translate.Key("Close"));
                a.WithAttribute(BootstrapHelper.DataDismiss, "alert");
                a.WithCssClass(BootstrapHelper.Close);

            });
            //html.AppendLine($"\t<div class=\"alert {BootstrapHelper.Well}\" ");
            //html.Append(GetProps());
            //html.AppendLine(">");
            return html;

            //html.Append($"\t\t<a href=\"#\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"alert\" aria-label=\"");
            //html.Append(Translate.Key("Close"));
            //html.AppendLine($"\">{BootstrapHelper.CloseButtonTimes}</a>");
            //html.Append("\t\t");
            //html.AppendLine($"{new JJIcon(Icon).GetHtml()}<strong>");
            //html.AppendLine($"{Translate.Key(Title)}");
            //html.AppendLine($"</strong>");
            //html.AppendLine(GetSplittedMessages());

            //html.Append("\t</div>");

            //return html.ToString();
        }

        private HtmlElement GetAlert()
        {
            var html = new StringBuilder();

            html.AppendLine($"<div class=\"alert alert-{ClassType} {CssClass} alert-dismissible\" role=\"alert\" ");
            html.Append(GetProps());
            html.AppendLine(">");

            if (ShowCloseButton)
            {
                html.AppendLine($"<button type=\"button\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"alert\" aria-label=\"close\">");
                html.AppendLine($"<span aria-hidden=\"true\">{BootstrapHelper.CloseButtonTimes}</span>");
                html.AppendLine("</button>");
            }

            html.AppendLine($"{new JJIcon(Icon).GetHtml()}<strong>");
            if (!string.IsNullOrEmpty(Title))
            {
                html.AppendLine($"{Translate.Key(Title)}<br>");

            }
            html.AppendLine($"</strong>");
            html.AppendLine(GetSplittedMessages());
            html.AppendLine("</div>");

            return html.ToString();

        }

        private string GetProps()
        {
            var html = new StringBuilder();
            if (!string.IsNullOrEmpty(Name))
            {
                html.Append($"id=\"{Name}\" name=\"{Name}\"");
            }

            foreach (DictionaryEntry attr in Attributes)
            {
                html.Append(' ');
                html.Append(attr.Key);
                if (attr.Value != null)
                {
                    html.Append("=\"");
                    html.Append(attr.Value);
                    html.Append('"');
                }
            }

            return html.ToString();
        }

    }
}


