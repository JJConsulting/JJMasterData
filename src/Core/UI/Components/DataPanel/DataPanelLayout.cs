using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http.Abstractions;
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
    public IFormValues FormValues { get; set; }
    
    public PageState PageState { get; set; }
    public DataPanelLayout(JJDataPanel dataPanel)
    {
        DataPanelControl = new DataPanelControl(dataPanel);
        RenderPanelGroup = dataPanel.RenderPanelGroup;
        FormElement = dataPanel.FormElement;
        Name = dataPanel.Name;
        PageState = dataPanel.PageState;
        FormValues = dataPanel.CurrentContext.Request.Form;
    }

    public async IAsyncEnumerable<HtmlBuilder> GetHtmlPanelList()
    {
        await foreach (var panel in GetTabPanels())
            yield return panel;

        await foreach (var nonTabPanel in GetNonTabPanels()) 
            yield return nonTabPanel;

        await foreach (var fieldWithoutPanel in GetFieldsWithoutPanel())
            yield return fieldWithoutPanel;
    }

    private async IAsyncEnumerable<HtmlBuilder> GetTabPanels()
    {

        var tabs = FormElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab);

        if (tabs.Count <= 0)
            yield break;
        
        var navTab = await GetTabNav(tabs);
        yield return navTab.GetHtmlBuilder();
    }

    private async IAsyncEnumerable<HtmlBuilder> GetNonTabPanels()
    {
        foreach (var panel in FormElement.Panels.Where(p => p.Layout != PanelLayout.Tab))
        {
            var htmlPanel = await GetHtmlPanelGroup(panel);
            if (htmlPanel != null)
                yield return htmlPanel;
        }
    }

    private async IAsyncEnumerable<HtmlBuilder> GetFieldsWithoutPanel()
    {
        bool dontContainsVisibleFields = !FormElement.Fields.ToList()
            .Exists(x => x.PanelId == 0 & !x.VisibleExpression.Equals("val:0"));
        
        if (dontContainsVisibleFields)
            yield break;
        
        if (!RenderPanelGroup)
        {
            yield return await GetHtmlForm(null);
        }
        else
        {
            var card = new JJCard
            {
                Layout = PanelLayout.Well,
                HtmlBuilderContent = await GetHtmlForm(null)
            };
            yield return card.GetHtmlBuilder();
        }
    }

    private async Task<JJTabNav> GetTabNav(List<FormElementPanel> tabs)
    {
        var navTab = new JJTabNav(FormValues)
        {
            Name = $"nav_{Name}"
        };
        foreach (var panel in tabs)
        {
            var htmlPanel = await GetHtmlForm(panel);
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

    private async Task<HtmlBuilder> GetHtmlPanelGroup(FormElementPanel panel)
    {
        var isVisible = await IsVisible(panel);
        if (!isVisible)
            return null;

        if (panel.Layout == PanelLayout.Collapse)
        {
            var collapse = new JJCollapsePanel(FormValues)
            {
                Title = panel.Title,
                SubTitle = panel.SubTitle,
                Name = $"{Name}-panel-{GuidGenerator.FromValue(panel.PanelId.ToString())}",
                CssClass = panel.CssClass,
                HtmlBuilderContent = await GetHtmlForm(panel),
                ExpandedByDefault = panel.ExpandedByDefault,
                Color = panel.Color
            };
            return collapse.GetHtmlBuilder();
        }

        var card = new JJCard
        {
            Title = panel.Title,
            SubTitle = panel.SubTitle,
            Layout = PanelLayout.Well,
            HtmlBuilderContent = await GetHtmlForm(panel)
        };
        return card.GetHtmlBuilder();
    }

    private async Task<HtmlBuilder> GetHtmlForm(FormElementPanel panel)
    {
        int panelId = panel?.PanelId ?? 0;
        
        var fields = FormElement.Fields
            .Where(x => x.PanelId == panelId)
            .OrderBy(x => x.LineGroup)
            .ToList();

        if (fields.Count == 0)
            return null;
        
        
        if (panel != null && !await IsEnabled(panel) || PageState is PageState.View)
        {
            foreach (var field in fields)
                field.EnableExpression = "val:0";
        }

        var formContent = await DataPanelControl.GetHtmlForm(fields);
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("container-fluid")
            .Append(formContent);

        return html;
    }

    private async Task<bool> IsEnabled(FormElementPanel panel)
    {
        bool panelEnable = await DataPanelControl.ExpressionsService.GetBoolValueAsync(
            panel.EnableExpression, DataPanelControl.FormState);

        return panelEnable;
    }

    private async Task<bool> IsVisible(FormElementPanel panel)
    {
        bool panelEnable = await DataPanelControl.ExpressionsService.GetBoolValueAsync(
            panel.VisibleExpression, DataPanelControl.FormState);

        return panelEnable;
    }

}
