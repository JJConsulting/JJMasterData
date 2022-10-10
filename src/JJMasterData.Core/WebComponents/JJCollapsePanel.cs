using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents;

public enum Position
{
    Right = 0,
    Left = 1
}

public class JJCollapsePanel : JJBaseView
{
    public Position ButtonPosition { get; set; }

    public string Title { get; set; }

    public JJIcon TitleIcon { get; set; }

    public string HtmlContent { get; set; }

    public HtmlElement HtmlElementContent { get; set; }

    public List<JJLinkButton> Buttons { get; set; }

    public bool ExpandedByDefault { get; set; }

    public PanelColor Color { get; set; }

    private bool IsCollapseOpen
    {
        get
        {
            var collapseMode = CurrentContext.Request["collapse_mode_" + Name];
            return string.IsNullOrEmpty(collapseMode) ? ExpandedByDefault : "1".Equals(collapseMode);
        }
    }

    public JJCollapsePanel()
    {
        ButtonPosition = Position.Right;
        Name = "collapse1";
        Buttons = new List<JJLinkButton>();
        Color = PanelColor.Default;
        TitleIcon = null;
    }

    internal override HtmlElement GetHtmlElement()
    {
        var root = new HtmlElement(HtmlTag.Div);

        root.AppendElement(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId($"collapse_mode_{Name}");
            input.WithValue(IsCollapseOpen ? "1" : "0");
        });

        root.AppendElement(BootstrapHelper.Version < 5 ? GetPanel() : GetAccordion());

        root.AppendScript($"setupCollapsePanel('{Name}')");

        return root;
    }

    #region Bootstrap 5 Accordion
    private HtmlElement GetAccordion()
    {
        var accordion = new HtmlElement(HtmlTag.Div)
                .WithCssClass("accordion pb-1")
                .WithAttribute("id", $"{Name}")
                .AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("accordion-item");
                    div.AppendElement(GetAccordionHeader());
                    div.AppendElement(GetAccordionBody());
                });
        return accordion;
    }

    private HtmlElement GetAccordionHeader()
    {
        var h2 = new HtmlElement(HtmlTag.H2)
        .WithCssClass(
            $"accordion-header bg-{Color.ToString().ToLower().Replace("default", "jjmasterdata")}")
        .WithAttribute("id", $"heading-{Name.ToLower()}")
        .AppendElement(HtmlTag.Button, button =>
        {
            button.WithCssClass($"accordion-button {(!IsCollapseOpen ? "collapsed" : "")}");
            button.WithAttribute("type", "button");
            button.WithAttribute("id", $"heading-{Name.ToLower()}");
            button.WithDataAttribute("toggle", "collapse");
            button.WithDataAttribute("target", $"#collapse-{Name.ToLower()}");
            button.AppendElement(TitleIcon);
            button.AppendTextIf(TitleIcon != null, "&nbsp;");
            button.AppendText(Translate.Key(Title));
        });

        return h2;
    }
    private HtmlElement GetAccordionBody()
    {
        var body = new HtmlElement(HtmlTag.Div)
            .WithAttribute("id", $"collapse-{Name.ToLower()}")
            .WithCssClass("accordion-collapse collapse")
            .WithCssClassIf(IsCollapseOpen, BootstrapHelper.Show)
            .WithAttribute("aria-labelledby", $"heading-{Name.ToLower()}")
            .WithDataAttribute("parent", $"#{Name}")
            .AppendElement(GetBody());
        return body;
    }
    #endregion

    #region Bootstrap 3/4 Panel
    private HtmlElement GetPanel()
    {
        var panel = new HtmlElement(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.PanelGroup)
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.GetPanel(Color.ToString().ToLower()));
                div.AppendElement(GetPanelHeading());
                div.AppendElement(HtmlTag.Div, body =>
                {
                    body.WithNameAndId(Name)
                    .WithCssClass("panel-collapse collapse")
                    .WithCssClassIf(IsCollapseOpen, BootstrapHelper.Show)
                    .AppendElement(GetBody());
                });
            });
        return panel;
    }
    private HtmlElement GetPanelHeading()
    {
        var panelHeading = new HtmlElement(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.GetPanelHeading(Color.ToString().ToLower()))
            .WithAttribute("href", "#collapseOne")
            .WithDataAttribute("toggle", "collapse")
            .WithDataAttribute("target", "#" + Name)
            .WithAttribute("aria-expanded", IsCollapseOpen.ToString().ToLower())
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass($"{BootstrapHelper.PanelTitle} unselectable");
                div.AppendElement(HtmlTag.A, a =>
                {
                    a.AppendElement(TitleIcon);
                    a.AppendTextIf(TitleIcon != null, "&nbsp;");
                    a.AppendText(Translate.Key(Title));
                });
            });
        return panelHeading;
    }
    private HtmlElement GetPanelBody()
    {
        var collapse = new HtmlElement(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithCssClass("panel-collapse collapse")
            .WithCssClassIf(IsCollapseOpen, "in show")
            .AppendElement(GetBody());

        return collapse;
    }
    #endregion

    private HtmlElement GetBody()
    {
        var panelBody = new HtmlElement(HtmlTag.Div)
            .AppendTextIf(!string.IsNullOrEmpty(HtmlContent), HtmlContent)
            .AppendElementIf(HtmlElementContent != null, HtmlElementContent)
            .AppendElementIf(Buttons.Count > 0, GetButtons())
            .WithCssClass(CssClass);

        panelBody.WithCssClass(BootstrapHelper.Version >= 5 ? "accordion-body" : BootstrapHelper.PanelBody);

        return panelBody;
    }

    private HtmlElement GetButtons()
    {
        if (Buttons.Count == 0)
            return null;

        var html = new HtmlElement(HtmlTag.Div)
            .WithCssClass("row")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("col-md-12");
                div.WithCssClassIf(ButtonPosition == Position.Left, BootstrapHelper.TextLeft);
                div.WithCssClassIf(ButtonPosition == Position.Right, BootstrapHelper.TextRight);

                foreach (var btn in Buttons)
                {
                    div.AppendText("&nbsp;");
                    div.AppendElement(btn.GetHtmlElement().WithCssClass("ms-1"));
                }

            });

        return html;
    }

}