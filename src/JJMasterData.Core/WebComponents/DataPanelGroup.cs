using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.WebComponents;

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

    public List<HtmlElement> GetListHtmlPanel()
    {
        var list = new List<HtmlElement>();

        var tabs = FormElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab);
        if (tabs.Count > 0)
        {
            var navTab = GetTabNav(tabs);
            list.Add(navTab.GetHtmlElement());
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
                card.HtmlElementContent = GetHtmlForm(null);
                list.Add(card.GetHtmlElement());
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

    private HtmlElement GetHtmlPanelGroup(FormElementPanel panel)
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
                HtmlElementContent = GetHtmlForm(panel),
                ExpandedByDefault = panel.ExpandedByDefault,
                Color = panel.Color
            };
            return collapse.GetHtmlElement();
        }
        else
        {
            var card = new JJCard
            {
                Title = panel.Title,
                SubTitle = panel.SubTitle,
                ShowAsWell = panel.Layout == PanelLayout.Well,
                HtmlElementContent = GetHtmlForm(panel)
            };
            return card.GetHtmlElement();
        }
    }

    private HtmlElement GetHtmlForm(FormElementPanel panel)
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
        var html = new HtmlElement(HtmlTag.Div)
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
