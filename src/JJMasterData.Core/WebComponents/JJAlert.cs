using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents
{
    public class JJAlert : JJBaseView
    {
        public PanelColor Color { get; set; }
        public IconType Icon { get; set; }
        public string Title { get; set; }
        public List<string> Messages { get; set; }
        public bool ShowCloseButton { get; set; }
        
        /// <remarks>
        /// Default = true
        /// </remarks>
        public bool ShowIcon { get; set; } = true;

        public JJAlert()
        {
            Messages = new List<string>();
        }

        internal override HtmlElement RenderHtmlElement()
        {
            var html = new HtmlElement(HtmlTag.Div)
                .WithNameAndId(Name)
                .WithAttributes(Attributes)
                .WithCssClass(CssClass)
                .WithCssClass($"alert alert-dismissible")
                .WithCssClass(GetClassType())
                .WithAttribute("role", "alert")
                .AppendElementIf(ShowCloseButton, GetCloseButton("alert"));

            if (ShowIcon)
                html.AppendElement(new JJIcon(Icon));

            html.AppendElementIf(!string.IsNullOrEmpty(Title), HtmlTag.Strong, e =>
                {
                    e.AppendText($"&nbsp;&nbsp;{Translate.Key(Title)}");
                });

            foreach(string message in Messages)
            {
                html.AppendElement(HtmlTag.Br);
                html.AppendText(Translate.Key(message));
            }

            return html;
        }

        private string GetClassType()
        {
            if (Color == PanelColor.Default)
                return BootstrapHelper.Version == 3 ? "well" : "alert-secondary";
            
            return $"alert-{Color.ToString().ToLower()}";
        } 

        internal static HtmlElement GetCloseButton(string dimissValue)
        {
            var btn = new HtmlElement(HtmlTag.Button)
                .WithAttribute("type", "button")
                .WithAttribute("aria-label", Translate.Key("Close"))
                .WithDataAttribute("dismiss", dimissValue)
                .WithCssClass(BootstrapHelper.Close)
                .AppendText(BootstrapHelper.CloseButtonTimes);

            return btn;

        }



    }
}