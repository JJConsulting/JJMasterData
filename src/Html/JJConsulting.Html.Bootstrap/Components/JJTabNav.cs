using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJTabNav : HtmlComponent
{
    public int SelectedTabIndex { get; set; }

    protected string InputHiddenSelectedTabName => $"selected_tab_{Name}";

    public List<NavContent> ListTab { get; set; }

    public TabsStyle Style { get; set; }

    public JJTabNav()
    {
        Name = "nav1";
        ListTab = [];
    }

    protected override HtmlBuilder BuildHtml()
    {
        var selectedTabIndex = GetSelectedTabIndex();
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .Append(GetNavTabs(selectedTabIndex))
            .Append(GetTabContent(selectedTabIndex))
            .Append(HtmlTag.Input, i =>
            {
                i.WithAttribute("type", "hidden")
                 .WithNameAndId(InputHiddenSelectedTabName)
                 .WithAttribute("value", selectedTabIndex.ToString());
            });

        return html;
    }

    private int GetSelectedTabIndex() =>
        SelectedTabIndex >= 0 && SelectedTabIndex < ListTab.Count ? SelectedTabIndex : 0;

    private HtmlBuilder GetNavTabs(int selectedTabIndex)
    {
        var ul = new HtmlBuilder(HtmlTag.Ul);
        ul.WithAttribute("id", $"{Name}-tab")
            .WithAttribute("role", "tablist")
            .WithCssClass($"nav {Style.CssClass}");

        for (int i = 0; i < ListTab.Count; i++)
        {
            var tab = ListTab[i];
            string navId = $"{Name}_nav_{i}";
            string tabId = $"{navId}-tab";

            var index = i;
            ul.Append(HtmlTag.Li, tabItem =>
            {
                tabItem.WithAttribute("role", "presentation")
                    .WithCssClass("nav-item")
                    .Append(HtmlTag.Button, button =>
                    {
                        button.WithAttribute("id", tabId)
                            .WithAttribute("type", "button")
                            .WithAttribute("data-bs-toggle", "tab")
                            .WithAttribute("data-bs-target", $"#{navId}")
                            .WithAttribute("aria-controls", navId)
                            .WithAttribute("data-jj-tab-index", index.ToString())
                            .WithAttribute("data-jj-object-id", InputHiddenSelectedTabName)
                            .WithAttribute("aria-selected", selectedTabIndex == index ? "true" : "false")
                            .WithAttribute("role", "tab")
                            .WithCssClass("jj-tab-link nav-link")
                            .WithCssClassIf(selectedTabIndex == index, "active");

                        if (tab.Icon.HasValue)
                        {
                            button.AppendComponent(new JJIcon(tab.Icon.Value)
                            {
                                CssClass = "me-1"
                            });
                        }

                        button.AppendText(tab.Title);
                    });
            });
        }

        return ul;
    }

    private HtmlBuilder GetTabContent(int selectedTabIndex)
    {
        var tabContent = new HtmlBuilder(HtmlTag.Div)
            .WithAttribute("id", $"{Name}-tabContent")
            .WithCssClass("tab-content");

        for (int i = 0; i < ListTab.Count; i++)
        {
            var tab = ListTab[i];
            var divContent = new HtmlBuilder(HtmlTag.Div)
                .WithAttribute("id", $"{Name}_nav_{i}")
                .WithAttribute("role", "tabpanel")
                .WithAttribute("aria-labelledby", $"{Name}_nav_{i}-tab")
                .WithAttribute("tabindex", "0")
                .WithCssClass("tab-pane fade")
                .WithCssClassIf(selectedTabIndex == i, "show active")
                .Append(tab.Content);

            tabContent.Append(divContent);
        }

        return tabContent;
    }
}
