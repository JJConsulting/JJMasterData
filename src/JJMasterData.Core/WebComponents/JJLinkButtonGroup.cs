using JJMasterData.Commons.Language;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;


namespace JJMasterData.Core.WebComponents
{
    public class JJLinkButtonGroup : JJBaseView
    {
        private List<JJLinkButton> _actions;

        /// <summary>
        /// Actions of input
        /// </summary>
        public List<JJLinkButton> Actions
        {
            get
            {
                if (_actions == null)
                    _actions = new List<JJLinkButton>();

                return _actions;
            }
            set { _actions = value; }
        }

        public bool ShowAsButton { get; set; }

        public string CaretText { get; set; }

        public JJLinkButtonGroup()
        {
        }

        internal override HtmlElement GetHtmlElement()
        {
            var inputGroup = new HtmlElement(HtmlTag.Div)
                .WithAttributes(Attributes)
                .WithNameAndId(Name)
                .WithCssClass("btn-group")
                .WithCssClass(CssClass);

            AddActionsAt(inputGroup);

            return inputGroup;
        }

        internal void AddActionsAt(HtmlElement inputGroup)
        {
            var listAction = Actions.ToList().FindAll(x => !x.IsGroup && x.Visible);
            var listActionGroup = Actions.ToList().FindAll(x => x.IsGroup && x.Visible);

            if (listAction.Count == 0 && listActionGroup.Count == 0)
                return;

            foreach (var action in listAction)
            {
                action.ShowAsButton = ShowAsButton;
                inputGroup.AppendElement(action.GetHtmlElement());
            }

            if (listActionGroup.Count > 0)
            {
                inputGroup.AppendElement(GetHtmlCaretButton());
                inputGroup.AppendElement(HtmlTag.Ul, ul =>
                {
                    ul.WithCssClass("dropdown-menu dropdown-menu-right");
                    AddGroupActions(ul, listActionGroup);
                });
            }
        }

        private void AddGroupActions(HtmlElement ul, List<JJLinkButton> listAction)
        {
            foreach (var action in listAction)
            {
                action.ShowAsButton = false;

                if (action.DividerLine)
                {
                    ul.AppendElement(HtmlTag.Li, li =>
                    {
                        li.WithAttribute("role", "separator").WithCssClass("divider dropdown-divider");
                    });
                }

                ul.AppendElement(HtmlTag.Li, li =>
                {
                    li.WithCssClass("dropdown-item").AppendElement(action.GetHtmlElement());
                });
            }
        }

        private HtmlElement GetHtmlCaretButton()
        {
            var html = new HtmlElement(HtmlTag.A)
                .WithAttribute("href", "#")
                .WithAttribute(BootstrapHelper.DataToggle, "dropdown")
                .WithAttribute("aria-haspopup", "true")
                .WithAttribute("aria-expanded", "false")
                .WithCssClass("dropdown-toggle")
                .WithCssClassIf(ShowAsButton, BootstrapHelper.DefaultButton)
                .AppendTextIf(!string.IsNullOrEmpty(CaretText), CaretText)
                .AppendElementIf(BootstrapHelper.Version == 3, HtmlTag.Span, s =>
                {
                    s.WithCssClass("caret")
                        .WithToolTip(Translate.Key("More Options"));
                });
            
            return html;
        }

    }
}
