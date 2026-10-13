#nullable disable warnings
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Render panels with fields
/// </summary>
internal sealed class DataPanelLayout(JJDataPanel dataPanel)
{
    private readonly string _name = dataPanel.Name;
    private readonly bool _renderPanelGroup = dataPanel.RenderPanelGroup;
    private readonly FormElement _formElement = dataPanel.FormElement;
    private readonly DataPanelForm _dataPanelForm = new(dataPanel);
    private readonly IHttpContextAccessor _formValues = dataPanel.CurrentContext;
    private readonly PageState _pageState = dataPanel.PageState;
    private readonly ExpressionsService _expressionsService= dataPanel.ExpressionsService;
    
    public async Task<HtmlBuilder> GetPanelHtml()
    {
        var panels = await GetTabPanelsHtml();

        panels.AppendRange(await GetNonTabPanels());
        panels.AppendRange(await GetFieldsWithoutPanel());

        return panels;
    }

    private async Task<HtmlBuilder> GetTabPanelsHtml()
    {
        var tabs = _formElement.Panels.FindAll(x => x.Layout == PanelLayout.Tab && x.ParentId == null);

        if (tabs.Count == 0)
            return new HtmlBuilder();
        
        var navTab = await GetTabNav(tabs);
        return navTab.GetHtmlBuilder();
    }

    private async Task<List<HtmlBuilder>> GetNonTabPanels()
    {
        List<HtmlBuilder> htmlList = [];
        foreach (var panel in _formElement.Panels.Where(p => p.Layout != PanelLayout.Tab && p.ParentId == null))
        {
            var htmlPanel = await GetPanelHtml(panel);
            if (htmlPanel != null)
                htmlList.Add(htmlPanel);
        }

        return htmlList;
    }

    private async Task<List<HtmlBuilder>> GetFieldsWithoutPanel()
    {
        List<HtmlBuilder> htmlList = [];
        var dontContainsVisibleFields = !_formElement.Fields.Exists(x => x.PanelId == 0 && _expressionsService.GetBoolValue(x.VisibleExpression, _dataPanelForm.FormStateData));
    
        if (dontContainsVisibleFields)
            return htmlList;
    
        if (!_renderPanelGroup)
            htmlList.Add(await GetHtmlForm(null));
        else
        {
            var card = new JJCard
            {
                Content = await GetHtmlForm(null)
            };
            htmlList.Add(card.GetHtmlBuilder());
        }
    
        return htmlList;
    }

    private async Task<JJTabNav> GetTabNav(List<FormElementPanel> tabs)
    {
        var navTab = new JJMasterDataTabNav(_formValues, $"nav_{_name}")
        {
            Style = _formElement.Options.Form.TabsStyle
        };
        
        foreach (var panel in tabs)
        {
            if (!IsVisible(panel))
                continue;

            var htmlPanel = await GetPanelContent(panel);
            if (htmlPanel != null)
            {
                var tabContent = new NavContent
                {
                    Title = GetPanelExpression(panel.Title),
                    Icon = panel.Icon,
                    Content = htmlPanel
                };
                navTab.ListTab.Add(tabContent);
            }
        }

        return navTab;
    }
    
    private string GetPanelExpression(string? expression)
    {
        var value = dataPanel
            .ExpressionsService
            .GetExpressionValue(expression, _dataPanelForm.FormStateData)
            ?.ToString() ?? string.Empty;

        return dataPanel.StringLocalizer[value];
    }

    private async Task<HtmlBuilder?> GetPanelHtml(FormElementPanel panel)
    {
        var isVisible = IsVisible(panel);
        if (!isVisible)
            return null;

        if (panel.Layout == PanelLayout.Collapse)
        {
            var collapse = new JJMasterDataCollapsePanel(_formValues)
            {
                Title = GetPanelExpression(panel.Title),
                SubTitle =GetPanelExpression(panel.SubTitle),
                TitleIcon = panel.Icon.HasValue ? new JJIcon(panel.Icon.Value) : null,
                Name = $"{_name}-panel-{GuidGenerator.FromValue(panel.PanelId.ToString())}",
                CssClass = panel.CssClass,
                Content = await GetPanelContent(panel),
                ExpandedByDefault = panel.ExpandedByDefault,
                Color = panel.Color
            };
            return collapse.GetHtmlBuilder();
        }
        else if (panel.Layout == PanelLayout.Card)
        {
            var card = new JJCard
            {
                Title = GetPanelExpression(panel.Title),
                SubTitle = GetPanelExpression(panel.SubTitle),
                Icon = panel.Icon,
                CssClass = panel.CssClass,
                Content = await GetPanelContent(panel)
            };
            return card.GetHtmlBuilder();
        }
        else if (panel.Layout == PanelLayout.NoDecoration)
        {
            var container = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(panel.CssClass);

            if (!string.IsNullOrEmpty(panel.Title) || !string.IsNullOrEmpty(panel.SubTitle))
            {
                var title = new JJTitle
                {
                    Title = GetPanelExpression(panel.Title),
                    SubTitle = GetPanelExpression(panel.SubTitle),
                    Icon = panel.Icon,
                    Size = HeadingSize.H3
                };

                container.AppendComponent(title);
            }
            
            container.Append(await GetPanelContent(panel));
            
            return container;
        }
        else
        {
            return await GetPanelContent(panel);
        }
    }

    private async Task<HtmlBuilder?> GetHtmlForm(FormElementPanel? panel)
    {
        var panelId = panel?.PanelId ?? 0;
        
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

        var formContent = await _dataPanelForm.GetHtmlForm(fields);
        var html = new HtmlBuilder(HtmlTag.Div).Append(formContent);

        return html;
    }

    private async Task<HtmlBuilder?> GetPanelContent(FormElementPanel panel)
    {
        var contents = new List<HtmlBuilder>();
        var form = await GetHtmlForm(panel);
        contents.Add(form);

        var children = _formElement.Panels.Where(x => x.ParentId == panel.PanelId).ToList();
        var childTabs = children.Where(x => x.Layout == PanelLayout.Tab).ToList();
        if (childTabs.Count > 0)
        {
            var tabs = await GetTabNav(childTabs);
            contents.Add(tabs.GetHtmlBuilder());
        }

        foreach (var child in children.Where(x => x.Layout != PanelLayout.Tab))
        {
            var html = await GetPanelHtml(child);
            contents.Add(html);
        }

        if (contents.Count == 0)
            return null;
        
        var container = new HtmlBuilder(HtmlTag.Div).WithCssClass("container");
        
        container.AppendDiv(div =>
        {
            div.WithCssClass("row");
            foreach (var content in contents)
                div.Append(content);
        });
        
        return container;
    }

    private bool IsEnabled(FormElementPanel panel)
    {
        return _dataPanelForm.ExpressionsService.GetBoolValue(panel.EnableExpression, _dataPanelForm.FormStateData);
    }

    private bool IsVisible(FormElementPanel panel)
    {
        return _dataPanelForm.ExpressionsService.GetBoolValue(panel.VisibleExpression, _dataPanelForm.FormStateData);
    }

}
