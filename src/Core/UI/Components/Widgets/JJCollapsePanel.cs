﻿using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJCollapsePanel : HtmlComponent
{
    public enum Position
    {
        Right = 0,
        Left = 1
    }
    public Position ButtonPosition { get; set; }

    public string Title { get; set; }

    public string SubTitle { get; set; }

    public JJIcon TitleIcon { get; set; }

    public string HtmlContent { get; set; }

    public HtmlBuilder HtmlBuilderContent { get; set; }

    public List<JJLinkButton> Buttons { get; set; }

    public bool ExpandedByDefault { get; set; }

    public PanelColor Color { get; set; }

    internal IHttpContext CurrentContext { get; }
    
    private bool IsCollapseOpen
    {
        get
        {
            var collapseMode = CurrentContext.Request["collapse_mode_" + Name];
            return string.IsNullOrEmpty(collapseMode) ? ExpandedByDefault : "1".Equals(collapseMode);
        }
    }

    public JJCollapsePanel(IHttpContext currentContext)
    {
        CurrentContext = currentContext;
        ButtonPosition = Position.Right;
        Name = "collapse1";
        Buttons = new List<JJLinkButton>();
        Color = PanelColor.Default;
        TitleIcon = null;
    }

    internal override HtmlBuilder BuildHtml()
    {
        var root = new HtmlBuilder(HtmlTag.Div);

        root.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId($"collapse_mode_{Name}");
            input.WithValue(IsCollapseOpen ? "1" : "0");
        });

        root.Append(BootstrapHelper.Version < 5 ? GetPanel() : GetAccordion());

        root.AppendScript($"setupCollapsePanel('{Name}')");

        return root;
    }

    #region Bootstrap 5 Accordion
    private HtmlBuilder GetAccordion()
    {
        var accordion = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass("accordion pb-1 mb-3")
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
            button.AppendComponent(TitleIcon);
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
            .WithDataAttribute("target", "#" + Name)
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
            var title = new JJTitle(null, SubTitle);
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