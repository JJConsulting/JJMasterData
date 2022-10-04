using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

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

        if (BootstrapHelper.Version < 5)
        {
            root.AppendElement(GetPanel());
        }
        else
        {
            root.AppendElement(GetAccordion());
        }

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
            button.AppendText(Translate.Key(Title));
        });

        return h2;
    }
    private HtmlElement GetAccordionBody()
    {
        var body = new HtmlElement(HtmlTag.Div)
            .WithAttribute("id", $"collapse-{Name.ToLower()}")
            .WithCssClass($"accordion-collapse collapse {(IsCollapseOpen ? "show" : "")}")
            .WithAttribute("aria-labelledby", $"heading-{Name.ToLower()}")
            .WithDataAttribute("parent", $"#{Name}")
            .AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("accordion-body");
                    div.AppendText(HtmlContent);
                    div.AppendElement(HtmlTag.Div, div =>
                    {
                        div.WithCssClass("row");
                        div.AppendElement(HtmlTag.Div, div =>
                        {
                            div.WithCssClass("col-md-12");
                            div.WithCssClassIf(ButtonPosition == Position.Left, "text-start");
                            div.WithCssClassIf(ButtonPosition == Position.Right, "text-end");
                            div.AppendRange(Buttons.Select(btn => btn.GetHtmlElement().WithCssClass("ms-1"))
                                .ToList());
                        });
                    });
                }
            );
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
                div.AppendElement(GetPanelBody());
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
                    a.AppendElementIf(TitleIcon != null, TitleIcon?.GetHtmlElement());
                    a.AppendTextIf(TitleIcon != null, "&nbsp;");
                    a.AppendText(Title);
                });
            });
        return panelHeading;
    }
    private HtmlElement GetPanelBody()
    {
        var panelBody = new HtmlElement(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.PanelBody} {CssClass}")
            .AppendText(HtmlContent)
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClassIf(ButtonPosition is Position.Left, BootstrapHelper.TextLeft);
                div.WithCssClassIf(ButtonPosition is Position.Right, BootstrapHelper.TextRight);
                foreach (var btn in Buttons)
                {
                    div.AppendText("&nbsp;");
                    div.AppendElement(btn.GetHtmlElement());
                }
            });

        var collapse = new HtmlElement(HtmlTag.Div)
            .WithNameAndId(Name)
            .WithCssClass("panel-collapse collapse")
            .WithCssClassIf(IsCollapseOpen, "in show").AppendElement(panelBody);

        return collapse;
    }
    #endregion
}