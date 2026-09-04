using System;
using System.Collections.Generic;
using System.Linq;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Events.Args;


namespace JJMasterData.Core.UI.Components;

internal sealed class GridToolbar(JJGridView gridView)
{
    internal event EventHandler<GridToolbarActionEventArgs>? OnRenderToolbarAction;
    
    public HtmlBuilder GetHtmlBuilder(FormStateData formStateData)
    {
        var toolbar = new JJToolbar();

        AddActionsToToolbar(toolbar, formStateData);
            
        return toolbar.GetHtmlBuilder().WithCssClass("mb-1");
    }

    private void AddActionsToToolbar(JJToolbar toolbar, FormStateData formStateData)
    {
        var actions = gridView
            .ToolbarActions
            .OrderBy(a => a.Order);
        
        var actionButtonFactory = gridView.ComponentFactory.ActionButton;

        var groupedActions = new List<JJLinkButton>();
        
        foreach (var action in actions)
        {
            var linkButton = actionButtonFactory.CreateGridToolbarButton(action, gridView,formStateData);
            string? processingIndicatorId = null;
            string? processingIndicatorClass = null;
            if (!linkButton.Visible)
                continue;

            switch (action)
            {
                case InsertAction { ShowOpenedAtGrid: true }:
                    continue;
                case FilterAction { EnableScreenSearch: true }:
                    toolbar.Items.Add(gridView.Filter.GetHtmlToolBarSearch());
                    continue;
            }

            switch (action)
            {
                case ExportAction when gridView.DataExportation.IsRunning():
                    processingIndicatorId = $"data-exportation-action-spinner-{gridView.Name}";
                    processingIndicatorClass = "data-exportation-action-indicator";
                    break;
                case ImportAction when gridView.DataImportation.IsRunning():
                    processingIndicatorId = $"data-importation-action-spinner-{gridView.Name}";
                    processingIndicatorClass = "data-importation-action-indicator";
                    break;
                case FilterAction fAction:
                    if (fAction.ShowAsCollapse)
                        linkButton.Visible = false;
                    break;
            }

            if (processingIndicatorId is not null)
                linkButton.Attributes["aria-busy"] = "true";

            if (OnRenderToolbarAction is not null)
            {
                var args = new GridToolbarActionEventArgs(action, linkButton);
                OnRenderToolbarAction(gridView, args);

                if (args.HtmlResult is not null)
                {
                    toolbar.Items.Add(new HtmlBuilder(args.HtmlResult));
                    continue;
                }
            }

            var linkButtonHtml = linkButton.GetHtmlBuilder();
            if (processingIndicatorId is not null)
            {
                linkButtonHtml.Append(HtmlTag.Span, indicator => indicator
                    .WithId(processingIndicatorId)
                    .WithCssClass($"spinner-border data-operation-action-indicator {processingIndicatorClass}")
                    .WithAttribute("role", "status")
                    .WithAttribute("aria-hidden", "true"));
            }

            if(!action.IsGroup)
                toolbar.Items.Add(linkButtonHtml);
            else
                groupedActions.Add(linkButton);
        }

        if (groupedActions.Count > 0)
        {
            var groupedAction = new JJLinkButtonGroup
            {
                ShowAsButton = true,
                CaretHtml = new HtmlBuilder(gridView.StringLocalizer["More"], encode:false),
                CssClass = BootstrapHelper.PullRight,
                Actions = groupedActions
            };

            toolbar.Items.Add(groupedAction.GetHtmlBuilder());
        }
    }
}
