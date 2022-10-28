using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Render panels with fields
/// </summary>
internal class DataPanelGroup
{
    public string Name { get; set; }

    public bool RenderPanelGroup { get; set; }

    public FormElement FormElement { private get; set; }

    public DataPanelControl DataPanelControl { get; set; }

    public DataPanelGroup(JJDataPanel dataPanel)
    {
        DataPanelControl = new DataPanelControl(dataPanel);
        RenderPanelGroup = dataPanel.RenderPanelGroup;
        FormElement = dataPanel.FormElement;
        Name = dataPanel.Name;
    }

    public List<HtmlBuilder> GetListHtmlPanel()
    {
        var list = new List<HtmlBuilder>();

        var tabs = FormElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab);
        if (tabs.Count > 0)
        {
            var navTab = GetTabNav(tabs);
            list.Add(navTab.GetHtmlBuilder());
        }

        //Render other layout types
        foreach (FormElementPanel panel in FormElement.Panels)
        {
            if (panel.Layout != PanelLayout.Tab)
            {
                var htmlPanel = GetHtmlPanelGroup(panel);
                if (htmlPanel != null)
                    list.Add(htmlPanel);
            }
        }

        //Render fields without panel
        if (FormElement.Fields.ToList().Exists(x => x.PanelId == 0 & !x.VisibleExpression.Equals("val:0")))
        {
            if (!RenderPanelGroup)
            {
                list.Add(GetHtmlForm(null));
            }
            else
            {
                var card = new JJCard();
                card.ShowAsWell = true;
                card.HtmlBuilderContent = GetHtmlForm(null);
                list.Add(card.GetHtmlBuilder());
            }
        }

        return list;
    }

    private JJTabNav GetTabNav(List<FormElementPanel> tabs)
    {
        var navTab = new JJTabNav
        {
            Name = "nav_" + Name
        };
        foreach (FormElementPanel panel in tabs)
        {
            var htmlPanel = GetHtmlForm(panel);
            if (htmlPanel != null)
            {
                var tabContent = new NavContent
                {
                    Title = panel.Title,
                    HtmlContent = htmlPanel
                };
                navTab.ListTab.Add(tabContent);
            }
        }

        return navTab;
    }

    private HtmlBuilder GetHtmlPanelGroup(FormElementPanel panel)
    {
        if (!IsVisible(panel))
            return null;

        if (panel.Layout == PanelLayout.Collapse)
        {
            var collapse = new JJCollapsePanel
            {
                Title = panel.Title,
                SubTitle = panel.SubTitle,
                Name = Name + "_panel" + panel.PanelId,
                CssClass = panel.CssClass,
                HtmlBuilderContent = GetHtmlForm(panel),
                ExpandedByDefault = panel.ExpandedByDefault,
                Color = panel.Color
            };
            return collapse.GetHtmlBuilder();
        }
        else
        {
            var card = new JJCard
            {
                Title = panel.Title,
                SubTitle = panel.SubTitle,
                ShowAsWell = panel.Layout == PanelLayout.Well,
                HtmlBuilderContent = GetHtmlForm(panel)
            };
            return card.GetHtmlBuilder();
        }
    }

    private HtmlBuilder GetHtmlForm(FormElementPanel panel)
    {
        int panelId = panel == null ? 0 : panel.PanelId;
        var fields = FormElement.Fields.ToList()
            .FindAll(x => x.PanelId == panelId)
            .OrderBy(x => x.LineGroup)
            .ThenBy(x => x.Order)
            .ToList()
            .DeepCopy();

        if (fields.Count == 0)
            return null;

        if (panel != null && !IsEnabled(panel))
        {
            foreach (var field in fields)
                field.EnableExpression = "val:0";
        }

        var contentform = DataPanelControl.GetHtmlForm(fields);
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("container-fluid")
            .AppendElement(contentform);

        return html;
    }

    private bool IsEnabled(FormElementPanel panel)
    {
        var fieldManager = DataPanelControl.FieldManager;
        bool panelEnable = fieldManager.Expression.GetBoolValue(
            panel.EnableExpression, "Panel " + panel.Title, DataPanelControl.PageState, DataPanelControl.Values);

        return panelEnable;
    }

    private bool IsVisible(FormElementPanel panel)
    {
        var fieldManager = DataPanelControl.FieldManager;
        bool panelEnable = fieldManager.Expression.GetBoolValue(
            panel.VisibleExpression, "Panel " + panel.Title, DataPanelControl.PageState, DataPanelControl.Values);

        return panelEnable;
    }

}
