using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Render panels with fields
/// </summary>
internal class DataPanelLayout(JJDataPanel dataPanel)
{
    private string Name { get; } = dataPanel.Name;

    private bool RenderPanelGroup { get; } = dataPanel.RenderPanelGroup;

    private FormElement FormElement {  get; } = dataPanel.FormElement;

    private DataPanelControl DataPanelControl { get; } = new(dataPanel);
    private IFormValues FormValues { get;  } = dataPanel.CurrentContext.Request.Form;

    private PageState PageState { get; } = dataPanel.PageState;

    private ExpressionsService ExpressionsService { get; } = dataPanel.ExpressionsService;
    
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
            .Exists(x => x.PanelId == 0 && ExpressionsService.GetBoolValue(x.VisibleExpression,DataPanelControl.FormStateData));
        
        if (dontContainsVisibleFields)
            yield break;
        
        if (!RenderPanelGroup)
            yield return await GetHtmlForm(null);
        
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
                    Title = GetPanelExpression(panel.Title),
                    Icon = panel.Icon,
                    HtmlContent = htmlPanel
                };
                navTab.ListTab.Add(tabContent);
            }
        }

        return navTab;
    }
    
    private string GetPanelExpression(string expression)
    {
        return dataPanel
            .ExpressionsService
            .GetExpressionValue(expression, DataPanelControl.FormStateData)
            ?.ToString() ?? string.Empty;
    }

    private async Task<HtmlBuilder> GetHtmlPanelGroup(FormElementPanel panel)
    {
        var isVisible = IsVisible(panel);
        if (!isVisible)
            return null;

        if (panel.Layout == PanelLayout.Collapse)
        {
            var collapse = new JJCollapsePanel(FormValues)
            {
                Title = GetPanelExpression(panel.Title),
                SubTitle =GetPanelExpression(panel.SubTitle),
                TitleIcon = panel.Icon.HasValue ? new JJIcon(panel.Icon.Value) : null,
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
            Title = GetPanelExpression(panel.Title),
            SubTitle = GetPanelExpression(panel.SubTitle),
            Icon = panel.Icon,
            Layout = panel.Layout,
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
        
        
        if (panel != null && !IsEnabled(panel) || PageState is PageState.View)
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

    private bool IsEnabled(FormElementPanel panel)
    {
        return DataPanelControl.ExpressionsService.GetBoolValue(
            panel.EnableExpression, DataPanelControl.FormStateData);
    }

    private bool IsVisible(FormElementPanel panel)
    {
        return DataPanelControl.ExpressionsService.GetBoolValue(
            panel.VisibleExpression, DataPanelControl.FormStateData);
    }

}
