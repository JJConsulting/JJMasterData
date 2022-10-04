using JJMasterData.Commons.Language;
using JJMasterData.Core.Html;
using System.Collections.Generic;

namespace JJMasterData.Core.WebComponents;

public class JJTabNav : JJBaseView
{
    private int? _selectedTabIndex;
    public int SelectedTabIndex
    {
        get
        {
            _selectedTabIndex ??= RequestSelectedTabIndex();

            return (int)_selectedTabIndex;
        }
        set => _selectedTabIndex = value;
    }

    internal string InputHiddenSelectedTabName => $"selected_tab_{Name}";

    public List<NavContent> ListTab { get; set; }

    public JJTabNav()
    {
        Name = "nav1";
        ListTab = new List<NavContent>();
    }

    internal override HtmlElement GetHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .AppendElement(GetNavTabs())
            .AppendElement(GetTabContent())
            .AppendElement(HtmlTag.Input, i =>
            {
                i.WithAttribute("type", "hidden")
                 .WithNameAndId(InputHiddenSelectedTabName)
                 .WithAttribute("value", SelectedTabIndex.ToString());
            });

        return html;
    }

    private HtmlElement GetNavTabs()
    {
        var ul = new HtmlElement(HtmlTag.Ul)
            .WithAttribute("role", "tablist")
            .WithCssClass("nav nav-tabs");

        for (int i = 0; i < ListTab.Count; i++)
        {
            NavContent nav = ListTab[i];
            string navId = $"{Name}_nav_{i}";

            ul.AppendElement(HtmlTag.Li, li =>
            {
                li.WithCssClass("nav-item")
                  .WithCssClassIf(SelectedTabIndex == i && BootstrapHelper.Version == 3, "active")
                  .WithAttribute("role", "presentation")
                  .AppendElement(HtmlTag.A, a =>
                  {
                      a.WithAttribute("href", $"#{navId}")
                       .WithAttribute("aria-controls", navId)
                       .WithAttribute("jj-tabindex", i.ToString())
                       .WithAttribute("jj-objectid", InputHiddenSelectedTabName)
                       .WithAttribute("aria-selected", SelectedTabIndex == i ? "true" : "false")
                       .WithAttribute("role", "tab")
                       .WithDataAttribute("toggle", "tab")
                       .WithCssClass("jj-tab-link nav-link")
                       .WithCssClassIf(SelectedTabIndex == i && BootstrapHelper.Version > 3, "active")
                       .AppendText(Translate.Key(nav.Title));
                  });
            });
        }

        return ul;
    }

    private HtmlElement GetTabContent()
    {
        var tabContent = new HtmlElement(HtmlTag.Div)
            .WithCssClass("tab-content");

        for (int i = 0; i < ListTab.Count; i++)
        {
            NavContent nav = ListTab[i];
            var divContent = new HtmlElement(HtmlTag.Div)
                .WithAttribute("id", $"{Name}_nav_{i}")
                .WithAttribute("role", "tabpanel")
                .WithCssClass("tab-pane fade")
                .WithCssClassIf(SelectedTabIndex == i, "in show active")
                .AppendText(nav.HtmlContent);

            tabContent.AppendElement(divContent);
        }

        return tabContent;
    }

    private int RequestSelectedTabIndex()
    {
        string tabIndex = CurrentContext.Request[InputHiddenSelectedTabName];
        return int.TryParse(tabIndex, out var nIndex) ? nIndex : 0;
    }

}
