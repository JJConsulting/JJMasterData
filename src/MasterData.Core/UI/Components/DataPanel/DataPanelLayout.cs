using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Render panels with fields
/// </summary>
internal sealed class DataPanelLayout(JJDataPanel dataPanel)
{
    private readonly string _name = dataPanel.Name;
    private readonly bool _renderPanelGroup = dataPanel.RenderPanelGroup;
    private readonly FormElement _formElement = dataPanel.FormElement;
    private readonly DataPanelControl _dataPanelControl = new(dataPanel);
    private readonly IFormValues _formValues = dataPanel.CurrentContext.Request.Form;
    private readonly PageState _pageState = dataPanel.PageState;
    private readonly ExpressionsService _expressionsService= dataPanel.ExpressionsService;
    
    public async Task<List<HtmlBuilder>> GetHtmlPanelList()
    {
        List<HtmlBuilder> panels =
        [
            await GetTabPanelsHtml()
        ];

        panels.AddRange(await GetNonTabPanels());
        panels.AddRange(await GetFieldsWithoutPanel());

        return panels;
    }

    [ItemCanBeNull]
    private async Task<HtmlBuilder> GetTabPanelsHtml()
    {
        var tabs = _formElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab);

        if (tabs.Count == 0)
            return null;
        
        var navTab = await GetTabNav(tabs);
        return navTab.GetHtmlBuilder();
    }

    private async Task<List<HtmlBuilder>> GetNonTabPanels()
    {
        List<HtmlBuilder> htmlList = [];
        foreach (var panel in _formElement.Panels.Where(p => p.Layout != PanelLayout.Tab))
        {
            var htmlPanel = await GetHtmlPanelGroup(panel);
            if (htmlPanel != null)
                htmlList.Add(htmlPanel);
        }

        return htmlList;
    }

    private async Task<List<HtmlBuilder>> GetFieldsWithoutPanel()
    {
        List<HtmlBuilder> htmlList = [];
        bool dontContainsVisibleFields = !_formElement.Fields.Exists(x => x.PanelId == 0 && _expressionsService.GetBoolValue(x.VisibleExpression, _dataPanelControl.FormStateData));
    
        if (dontContainsVisibleFields)
            return htmlList;
    
        if (!_renderPanelGroup)
            htmlList.Add(await GetHtmlForm(null));
        else
        {
            var card = new JJCard
            {
                Layout = PanelLayout.Well,
                HtmlBuilderContent = await GetHtmlForm(null)
            };
            htmlList.Add(card.GetHtmlBuilder());
        }
    
        return htmlList;
    }

    private async Task<JJTabNav> GetTabNav(List<FormElementPanel> tabs)
    {
        var navTab = new JJTabNav(_formValues)
        {
            Name = $"nav_{_name}"
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
            .GetExpressionValue(expression, _dataPanelControl.FormStateData)
            ?.ToString() ?? string.Empty;
    }

    private async Task<HtmlBuilder> GetHtmlPanelGroup(FormElementPanel panel)
    {
        var isVisible = IsVisible(panel);
        if (!isVisible)
            return null;

        if (panel.Layout == PanelLayout.Collapse)
        {
            var collapse = new JJCollapsePanel(_formValues)
            {
                Title = GetPanelExpression(panel.Title),
                SubTitle =GetPanelExpression(panel.SubTitle),
                TitleIcon = panel.Icon.HasValue ? new JJIcon(panel.Icon.Value) : null,
                Name = $"{_name}-panel-{GuidGenerator.FromValue(panel.PanelId.ToString())}",
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
        
        var fields = _formElement.Fields
            .Where(x => x.PanelId == panelId)
            .OrderBy(x => x.LineGroup)
            .ToList();

        if (fields.Count == 0)
            return null;
        
        
        if (_pageState is PageState.View || (panel != null && !IsEnabled(panel)))
        {
            foreach (var field in fields)
                field.EnableExpression = "val:0";
        }

        var formContent = await _dataPanelControl.GetHtmlForm(fields);
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("container-fluid")
            .Append(formContent);

        return html;
    }

    private bool IsEnabled(FormElementPanel panel)
    {
        return _dataPanelControl.ExpressionsService.GetBoolValue(
            panel.EnableExpression, _dataPanelControl.FormStateData);
    }

    private bool IsVisible(FormElementPanel panel)
    {
        return _dataPanelControl.ExpressionsService.GetBoolValue(
            panel.VisibleExpression, _dataPanelControl.FormStateData);
    }

}
