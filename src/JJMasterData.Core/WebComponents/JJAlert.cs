using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents
{
    public class JJAlert : JJBaseView
    {
        public PanelColor Color { get; set; }
        public IconType Icon { get; set; }
        public string Title { get; set; }
        public List<string> Messages { get; set; }
        public bool ShowCloseButton { get; set; }
        private string ClassType => Color.Equals(PanelColor.Default) ? "secondary" : Color.ToString().ToLower();

        public JJAlert()
        {
            Messages = new List<string>();
        }
        
        private string GetSplittedMessages() => string.Join("<br>", Messages.Select(Translate.Key));
        
        internal override HtmlElement GetHtmlElement()
        {
            if (BootstrapHelper.Version == 3 & (Color == PanelColor.Default || Color == PanelColor.Primary))
            {
                return GetAlertDefaultBs3();
            }
            
            return GetAlert();
        }

        private HtmlElement GetAlertDefaultBs3()
        {
            var html = new HtmlElement(HtmlTag.Div)
                .WithNameAndId(Name)
                .WithAttributes(Attributes)
                .WithCssClass(CssClass)
                .WithCssClass($"alert {BootstrapHelper.Well}")
                .AppendElement(HtmlTag.A, e =>
                {
                    e.WithAttribute("href", "#");
                    e.WithAttribute("aria-label", Translate.Key("Close"));
                    e.WithAttribute(BootstrapHelper.DataDismiss, "alert");
                    e.WithCssClass(BootstrapHelper.Close);
                    e.AppendText(BootstrapHelper.CloseButtonTimes);
                })
                .AppendElement(new JJIcon(Icon).GetHtmlElement())
                .AppendElementIf(!string.IsNullOrEmpty(Title),HtmlTag.Strong, e =>
                {
                    e.AppendText(Translate.Key(Title));
                })
                .AppendText(GetSplittedMessages());
            
            return html;
        }

        private HtmlElement GetAlert()
        {
            var html = new HtmlElement(HtmlTag.Div)
                .WithNameAndId(Name)
                .WithAttributes(Attributes)
                .WithCssClass(CssClass)
                .WithCssClass($"alert alert-{ClassType} alert-dismissible")
                .WithAttribute("role", "alert")
                
                .AppendElementIf(ShowCloseButton, HtmlTag.Button, e =>
                {
                    e.WithCssClass(BootstrapHelper.Close);
                    e.WithAttribute("type", "button");
                    e.WithAttribute(BootstrapHelper.DataDismiss, "alert");
                    e.WithAttribute("aria-label", "close");
                    e.AppendElement(HtmlTag.Span, element =>
                    {
                        element.WithAttribute("aria-hidden", "true");
                        element.AppendText(BootstrapHelper.CloseButtonTimes);
                    });
                })
                .AppendElement(new JJIcon(Icon).GetHtmlElement())
                .AppendElementIf(!string.IsNullOrEmpty(Title), HtmlTag.Strong, e =>
                {
                    e.AppendText(Translate.Key(Title));
                })
                .AppendText(GetSplittedMessages());;

            return html;
        }
        
    }
}