using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Bootstrap.Utils;
using JJConsulting.Html.Extensions;

namespace JJConsulting.Html.Bootstrap.Components;

public class JJTabNav : HtmlComponent
{
    public int SelectedTabIndex { get; set; }

    protected string InputHiddenSelectedTabName => $"selected_tab_{Name}";

    public List<NavContent> ListTab { get; set; }

    public JJTabNav()
    {
        Name = "nav1";
        ListTab = [];
    }

    protected override HtmlBuilder BuildHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithAttributes(Attributes)
            .WithCssClass(CssClass)
            .Append(GetNavTabs())
            .Append(GetTabContent())
            .Append(HtmlTag.Input, i =>
            {
                i.WithAttribute("type", "hidden")
                 .WithNameAndId(InputHiddenSelectedTabName)
                 .WithAttribute("value", SelectedTabIndex.ToString());
            });

        return html;
    }

    private HtmlBuilder GetNavTabs()
    {
        var ul = new HtmlBuilder(HtmlTag.Ul)
            .WithAttribute("role", "tablist")
            .WithCssClass("nav nav-tabs");

        for (int i = 0; i < ListTab.Count; i++)
        {
            var nav = ListTab[i];
            string navId = $"{Name}_nav_{i}";

            var index = i;
            ul.Append(HtmlTag.Li, li =>
            {
                li.WithCssClassIf(BootstrapHelper.Version > 3, "nav-item")
                  .WithCssClassIf(SelectedTabIndex == index && BootstrapHelper.Version == 3, "active")
                  .WithAttribute("role", "presentation")
                  .Append(HtmlTag.A, a =>
                  {
                      if (nav.Icon.HasValue)
                      {
                          a.AppendComponent(new JJIcon(nav.Icon.Value)
                          {
                              CssClass = "me -1"
                          });
                      }
                      a.WithAttribute("href", $"#{navId}")
                       .WithAttribute("aria-controls", navId)
                       .WithAttribute("jj-tabindex", index.ToString())
                       .WithAttribute("jj-objectid", InputHiddenSelectedTabName)
                       .WithAttribute("aria-selected", SelectedTabIndex == index ? "true" : "false")
                       .WithAttribute("role", "tab")
                       .WithDataAttribute("toggle", "tab")
                       .WithCssClass("jj-tab-link nav-link")
                       .WithCssClassIf(SelectedTabIndex == index && BootstrapHelper.Version > 3, "active")
                       .AppendText(nav.Title);
                  });
            });
        }

        return ul;
    }

    private HtmlBuilder GetTabContent()
    {
        var tabContent = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("tab-content");

        for (int i = 0; i < ListTab.Count; i++)
        {
            var nav = ListTab[i];
            var divContent = new HtmlBuilder(HtmlTag.Div)
                .WithAttribute("id", $"{Name}_nav_{i}")
                .WithAttribute("role", "tabpanel")
                .WithCssClass("tab-pane fade")
                .WithCssClassIf(SelectedTabIndex == i, $"active{BootstrapHelper.Show}")
                .Append(nav.Content);

            tabContent.Append(divContent);
        }

        return tabContent;
    }
}
