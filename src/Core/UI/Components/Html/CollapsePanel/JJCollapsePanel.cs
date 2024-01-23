using System.Collections.Generic;
using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJCollapsePanel : HtmlComponent
{
    public enum Position
    {
        Right = 0,
        Left = 1
    }
    public Position ButtonPosition { get; set; }

    [LocalizationRequired]
    public string Title { get; set; }

    [LocalizationRequired]
    public string SubTitle { get; set; }

    [CanBeNull] 
    public JJIcon TitleIcon { get; set; }

    public string HtmlContent { get; set; }

    public HtmlBuilder HtmlBuilderContent { get; set; }

    public List<JJLinkButton> Buttons { get; set; }

    public bool ExpandedByDefault { get; set; }

    public PanelColor Color { get; set; }

    internal IFormValues FormValues { get; }
    
    private bool IsCollapseOpen
    {
        get
        {
            var collapseMode = FormValues[$"{Name}-is-open"];
            return string.IsNullOrEmpty(collapseMode) ? ExpandedByDefault : "1".Equals(collapseMode);
        }
    }

    public JJCollapsePanel(IFormValues formValues)
    {
        FormValues = formValues;
        ButtonPosition = Position.Right;
        Name = "collapse1";
        Buttons = [];
        Color = PanelColor.Default;
        TitleIcon = null;
        HtmlBuilderContent = new HtmlBuilder();
    }

    internal override HtmlBuilder BuildHtml()
    {
        var root = new HtmlBuilder(HtmlTag.Div);

        root.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId($"{Name}-is-open");
            input.WithValue(IsCollapseOpen ? "1" : "0");
        });

        root.Append(BootstrapHelper.Version < 5 ? GetPanel() : GetAccordion());

        root.AppendScript($"CollapsePanelListener.listen('{Name}')");

        return root;
    }

    #region Bootstrap 5 Accordion
    private HtmlBuilder GetAccordion()
    {
        var accordion = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass($"accordion accordion-{Color.ToString().ToLower()} pb-1 mb-3")
                .WithAttribute("id", $"{Name}")
                .Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("accordion-item");
                    div.Append(GetAccordionHeader());
                    div.Append(GetAccordionBody());
                });
        return accordion;
    }

    private HtmlBuilder GetAccordionHeader()
    {
        var h2 = new HtmlBuilder(HtmlTag.H2)
        .WithCssClass(
            $"accordion-header ")
        .WithAttribute("id", $"heading-{Name.ToLower()}")
        .Append(HtmlTag.Button, button =>
        {
            button.WithCssClass($"accordion-button {(!IsCollapseOpen ? "collapsed" : "")}");
            button.WithAttribute("type", "button");
            button.WithAttribute("id", $"heading-{Name.ToLower()}");
            button.WithDataAttribute("toggle", "collapse");
            button.WithDataAttribute("target", $"#collapse-{Name.ToLower()}");
            button.AppendComponentIf(TitleIcon != null, TitleIcon);
            button.AppendTextIf(TitleIcon != null, "&nbsp;");
            button.AppendText(Title);
        });

        return h2;
    }
    private HtmlBuilder GetAccordionBody()
    {
        var body = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"collapse-{Name.ToLower()}")
            .WithCssClass("accordion-collapse collapse")
            .WithCssClassIf(IsCollapseOpen, BootstrapHelper.Show)
            .WithAttribute("aria-labelledby", $"heading-{Name.ToLower()}")
            .WithDataAttribute("parent", $"#{Name}")
            .Append(GetBody());
        return body;
    }
    #endregion

    #region Bootstrap 3/4 Panel
    private HtmlBuilder GetPanel()
    {
        var panel = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.PanelGroup)
            .Append(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.GetPanel(Color.ToString().ToLower()));
                div.Append(GetPanelHeading());
                div.Append(HtmlTag.Div, body =>
                {
                    body.WithNameAndId(Name)
                    .WithCssClass("panel-collapse collapse")
                    .WithCssClassIf(IsCollapseOpen, BootstrapHelper.Show)
                    .Append(GetBody());
                });
            });
        return panel;
    }
    private HtmlBuilder GetPanelHeading()
    {
        var panelHeading = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.GetPanelHeading(Color.ToString().ToLower()))
            .WithAttribute("href", "#collapseOne")
            .WithDataAttribute("toggle", "collapse")
            .WithDataAttribute("target", $"#{Name}")
            .WithAttribute("aria-expanded", IsCollapseOpen.ToString().ToLower())
            .Append(HtmlTag.Div, div =>
            {
                div.WithCssClass($"{BootstrapHelper.PanelTitle} unselectable");
                div.Append(HtmlTag.A, a =>
                {
                    a.AppendComponent(TitleIcon);
                    a.AppendTextIf(TitleIcon != null, "&nbsp;");
                    a.AppendText(Title);
                });
            });
        return panelHeading;
    }

    #endregion

    private HtmlBuilder GetBody()
    {
        var panelBody = new HtmlBuilder(HtmlTag.Div);

        if (!string.IsNullOrEmpty(SubTitle))
        {
            var title = new JJTitle
            {
                SubTitle = SubTitle
            };
            panelBody.Append(title.GetHtmlBlockquote());
        }

        panelBody.AppendTextIf(!string.IsNullOrEmpty(HtmlContent), HtmlContent);
        panelBody.AppendIf(HtmlBuilderContent != null,()=> HtmlBuilderContent);
        panelBody.AppendIf(Buttons.Count > 0,GetButtons);
        panelBody.WithCssClass(CssClass);
        panelBody.WithCssClass(BootstrapHelper.Version >= 5 ? "accordion-body" : BootstrapHelper.PanelBody);

        return panelBody;
    }

    private HtmlBuilder GetButtons()
    {
        if (Buttons.Count == 0)
            return null;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("col-md-12");
                div.WithCssClassIf(ButtonPosition == Position.Left, BootstrapHelper.TextLeft);
                div.WithCssClassIf(ButtonPosition == Position.Right, BootstrapHelper.TextRight);

                foreach (var btn in Buttons)
                {
                    div.AppendText("&nbsp;");
                    div.Append(btn.BuildHtml().WithCssClass("ms-1"));
                }

            });

        return html;
    }

}