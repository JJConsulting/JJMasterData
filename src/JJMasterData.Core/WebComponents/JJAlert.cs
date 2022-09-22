using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.WebComponents
{
    public class JJAlert : JJBaseView
    {

        public PanelColor Type { get; set; }
        public IconType Icon { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public List<string> ListMessages { get; set; }
        public string Style { get; set; }

        public bool ShowCloseButton { get; set; }

        private string ClassType
        {
            get
            {
                if (Type.Equals(PanelColor.Default))
                {
                    return "secondary";
                }
                return Type.ToString().ToLower();

            }
        }

        public JJAlert()
        {
            ListMessages = new List<string>();
        }

        protected override string RenderHtml()
        {
            if (BootstrapHelper.Version == 3 & (Type == PanelColor.Default || Type == PanelColor.Primary))
            {
                return GetAlertDefaultBs3();
            }
            else
            {
                return GetAlert();
            }

        }

        private string GetAlertDefaultBs3()
        {
            var html = new StringBuilder();

            html.AppendLine($"\t<div class=\"alert {BootstrapHelper.Well}\">");

            html.Append($"\t\t<a href=\"#\" class=\"{BootstrapHelper.Close}\" {BootstrapHelper.DataDismiss}=\"alert\" aria-label=\"");
            html.Append(Translate.Key("Close"));
            html.AppendLine($"\">{BootstrapHelper.CloseButtonTimes}</a>");
            html.Append("\t\t");
            html.AppendLine($"{new JJIcon(Icon).GetHtml()}<strong>");
            html.AppendLine($"{Translate.Key(Title)}");
            html.AppendLine($"</strong>");
            html.AppendLine($"{Translate.Key(Message)}");

            html.Append("\t</div>");

            return html.ToString();
        }

        private string GetAlert()
        {
            var html = new StringBuilder();

            html.AppendLine($"<div id=\"{Id}\" class=\"alert alert-{ClassType} {CssClass} alert-dismissible\" role=\"alert\" style=\"{Style}\">");

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
            html.AppendLine(GetMessages());
            html.AppendLine("</div>");

            return html.ToString();

        }

        private string GetMessages()
        {
            if (ListMessages.Count > 0)
            {
                return string.Join("<br>", ListMessages);
            }

            return Message;
        }
    }
}


