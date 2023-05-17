using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Render panels with fields
/// </summary>
internal class DataPanelLayout
{
    public string Name { get; set; }

    public bool RenderPanelGroup { get; set; }

    public FormElement FormElement { private get; set; }

    public DataPanelControl DataPanelControl { get; set; }

    public DataPanelLayout(JJDataPanel dataPanel)
    {
        DataPanelControl = new DataPanelControl(dataPanel);
        RenderPanelGroup = dataPanel.RenderPanelGroup;
        FormElement = dataPanel.FormElement;
        Name = dataPanel.Name;
    }

    public IEnumerable<HtmlBuilder> GetHtmlPanelList()
    {
        foreach (var panel in GetTabPanels())
            yield return panel;

        foreach (var nonTabPanel in GetNonTabPanels()) 
            yield return nonTabPanel;

        foreach (var fieldWithoutPanel in GetFieldsWithoutPanel())
            yield return fieldWithoutPanel;
    }

    private IEnumerable<HtmlBuilder> GetTabPanels()
    {

        var tabs = FormElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab);

        if (tabs.Count <= 0)
            yield break;
        
        var navTab = GetTabNav(tabs);
        yield return navTab.GetHtmlBuilder();
    }

    private IEnumerable<HtmlBuilder> GetNonTabPanels()
    {
        foreach (var panel in FormElement.Panels.Where(p => p.Layout != PanelLayout.Tab))
        {
            var htmlPanel = GetHtmlPanelGroup(panel);
            if (htmlPanel != null)
                yield return htmlPanel;
        }
    }

    private IEnumerable<HtmlBuilder> GetFieldsWithoutPanel()
    {
        bool dontContainsVisibleFields = !FormElement.Fields.ToList()
            .Exists(x => x.PanelId == 0 & !x.VisibleExpression.Equals("val:0"));
        
        if (dontContainsVisibleFields)
            yield break;
        
        if (!RenderPanelGroup)
        {
            yield return GetHtmlForm(null);
        }
        else
        {
            var card = new JJCard
            {
                ShowAsWell = true,
                HtmlBuilderContent = GetHtmlForm(null)
            };
            yield return card.GetHtmlBuilder();
        }
    }

    private JJTabNav GetTabNav(List<FormElementPanel> tabs)
    {
        var navTab = new JJTabNav
        {
            Name = "nav_" + Name
        };
        foreach (var panel in tabs)
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

        var card = new JJCard
        {
            Title = panel.Title,
            SubTitle = panel.SubTitle,
            ShowAsWell = panel.Layout == PanelLayout.Well,
            HtmlBuilderContent = GetHtmlForm(panel)
        };
        return card.GetHtmlBuilder();
    }

    private HtmlBuilder GetHtmlForm(FormElementPanel panel)
    {
        int panelId = panel?.PanelId ?? 0;
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

        var formContent = DataPanelControl.GetHtmlForm(fields);
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("container-fluid")
            .AppendElement(formContent);

        return html;
    }

    private bool IsEnabled(FormElementPanel panel)
    {
        var fieldManager = DataPanelControl.FieldManager;
        bool panelEnable = fieldManager.ExpressionManager.GetBoolValue(
            panel.EnableExpression, "Panel " + panel.Title, DataPanelControl.PageState, DataPanelControl.Values);

        return panelEnable;
    }

    private bool IsVisible(FormElementPanel panel)
    {
        var fieldManager = DataPanelControl.FieldManager;
        bool panelEnable = fieldManager.ExpressionManager.GetBoolValue(
            panel.VisibleExpression, "Panel " + panel.Title, DataPanelControl.PageState, DataPanelControl.Values);

        return panelEnable;
    }

}
