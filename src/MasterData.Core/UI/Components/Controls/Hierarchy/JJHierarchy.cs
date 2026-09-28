using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// A single-value tree control whose children are loaded on demand.
/// </summary>
public class JJHierarchy(
    IHttpContextAccessor httpContextAccessor,
    DataProtectionService dataProtectionService,
    DataItemService dataItemService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : ControlBase(httpContextAccessor), IDataItemControl
{
    private const int MaximumPathDepth = 100;

    public Guid? ConnectionId { get; set; }
    public FormElementDataItem DataItem { get; set; } = new();
    public FormStateData FormStateData { get; set; } = new([], PageState.List);
    public string? ElementName { get; set; }
    public string? ParentElementName { get; set; }
    internal string FieldName { get; set; } = string.Empty;
    public string HtmlId { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool ShowSelectedPathOnly { get; set; }

    public string? SelectedValue
    {
        get
        {
            if (field is null && HasFormValues)
                field = FormValues[Name];
            return field;
        }
        set;
    }

    private RouteContext RouteContext => field ??=
        new RouteContextFactory(httpContextAccessor, dataProtectionService).Create();

    private ComponentContext ComponentContext => RouteContext.ComponentContext;
    private bool CanInteract => Enabled && !ReadOnly && !ShowSelectedPathOnly;

    protected override async ValueTask<ComponentResult> BuildResultAsync()
    {
        var requestedField = httpContextAccessor.HttpContext?.Request.Query["fieldName"].ToString();
        if (ComponentContext is ComponentContext.Hierarchy or ComponentContext.HierarchyFilter &&
            FieldName.Equals(requestedField, StringComparison.OrdinalIgnoreCase))
        {
            return await GetItemsResultAsync();
        }

        return await base.BuildResultAsync();
    }

    protected internal override async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var htmlId = string.IsNullOrEmpty(HtmlId) ? Name.Replace('.', '_') : HtmlId;
        var panelId = $"{htmlId}-hierarchy-panel";
        var path = await GetSelectedPathAsync();
        var selectedItem = path.LastOrDefault();

        var wrapper = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("jj-hierarchy")
            .WithCssClass(CssClass)
            .WithAttribute("data-query-string", GetQueryString())
            .WithAttribute("data-value-input-id", $"{htmlId}-value")
            .WithAttribute("data-description-input-id", htmlId)
            .WithAttribute("data-panel-id", panelId)
            .WithAttribute("data-interactive", CanInteract ? "true" : "false")
            .WithAttribute("data-expand-label", stringLocalizer["Expand"])
            .WithAttribute("data-collapse-label", stringLocalizer["Collapse"]);

        wrapper.Append(HtmlTag.Input, input => input
            .WithAttribute("type", "hidden")
            .WithName(Name)
            .WithId($"{htmlId}-value")
            .WithValue(SelectedValue ?? string.Empty)
            .WithAttributeIf(!Enabled, "disabled", "disabled"));
        wrapper.Append(GetPickerHtml(htmlId, panelId, selectedItem));

        if (!string.IsNullOrEmpty(SelectedValue) && selectedItem is null)
        {
            wrapper.Append(HtmlTag.Div, warning => warning
                .WithCssClass("alert alert-warning py-2")
                .WithAttribute("role", "alert")
                .AppendText(stringLocalizer["The selected hierarchy item is no longer available."]));
        }

        var panel = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("card mt-2")
            .WithId(panelId)
            .WithAttributeIf(CanInteract, "hidden", "hidden");
        var cardBody = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("card-body");

        var tree = new HtmlBuilder(HtmlTag.Ul)
            .WithCssClass("jj-hierarchy-tree")
            .WithAttribute("role", "tree")
            .WithAttribute("aria-label", stringLocalizer["Hierarchy"]);

        if (ShowSelectedPathOnly)
        {
            AppendReadOnlyPath(tree, path, 0);
        }
        else
        {
            var roots = await GetChildrenAsync(null);
            await AppendLevelAsync(tree, roots, path, 0);
        }

        cardBody.Append(tree);
        if (CanInteract)
            cardBody.Append(GetActionsHtml(selectedItem));
        panel.Append(cardBody);
        wrapper.Append(panel);

        return wrapper;
    }

    private HtmlBuilder GetPickerHtml(string htmlId, string panelId, DataItemValue? selectedItem)
    {
        var inputGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group");
        var descriptionInput = new HtmlBuilder(HtmlTag.Input)
            .WithAttribute("type", "text")
            .WithCssClass("form-control")
            .WithId(htmlId)
            .WithValue(selectedItem?.Description ?? SelectedValue ?? string.Empty)
            .WithAttribute("placeholder", stringLocalizer["(Choose)"])
            .WithAttribute("aria-label", stringLocalizer["Hierarchy"])
            .WithAttribute("readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .WithAttributes(Attributes);

        inputGroup.Append(descriptionInput);
        if (CanInteract)
        {
            var openButton = new HtmlBuilder(HtmlTag.Button)
                .WithCssClass($"{BootstrapHelper.BtnDefault} jj-hierarchy-open")
                .WithAttribute("type", "button")
                .WithAttribute("aria-controls", panelId)
                .WithAttribute("aria-expanded", "false")
                .WithAttribute("aria-haspopup", "tree")
                .WithToolTip(stringLocalizer["Open hierarchy"])
                .Append(HtmlTag.Span, icon => icon
                    .WithCssClass("fa-solid fa-sitemap")
                    .WithAttribute("aria-hidden", "true"));

            if (BootstrapHelper.Version == 3)
            {
                inputGroup.Append(HtmlTag.Span, span => span
                    .WithCssClass("input-group-btn")
                    .Append(openButton));
            }
            else
            {
                inputGroup.Append(openButton);
            }
        }

        return inputGroup;
    }

    private async Task<JsonComponentResult> GetItemsResultAsync()
    {
        var request = httpContextAccessor.HttpContext!.Request;
        var parentId = request.Query["parentId"].ToString();
        var items = await GetChildrenAsync(string.IsNullOrEmpty(parentId) ? null : parentId);

        return new JsonComponentResult(items.Select(ToResult).ToList());
    }

    private async Task<List<DataItemValue>> GetSelectedPathAsync()
    {
        if (string.IsNullOrEmpty(SelectedValue))
            return [];

        try
        {
            return await dataItemService.GetHierarchyPathAsync(DataItem,
                new DataQuery(FormStateData, ConnectionId) { SearchId = SelectedValue }, MaximumPathDepth);
        }
        catch (JJMasterDataException)
        {
            return [];
        }
    }

    private Task<List<DataItemValue>> GetChildrenAsync(string? parentId) =>
        dataItemService.GetHierarchyValuesAsync(DataItem,
            new DataQuery(FormStateData, ConnectionId) { ParentId = parentId });

    private async Task AppendLevelAsync(
        HtmlBuilder parent,
        IReadOnlyCollection<DataItemValue> items,
        IReadOnlyList<DataItemValue> path,
        int depth)
    {
        foreach (var item in items)
        {
            var isSelected = item.Id.Equals(SelectedValue, StringComparison.OrdinalIgnoreCase);
            var isPathItem = depth < path.Count && item.Id.Equals(path[depth].Id, StringComparison.OrdinalIgnoreCase);
            var isExpanded = isPathItem && depth + 1 < path.Count;
            var listItem = GetNodeHtml(item, isSelected, isExpanded);
            parent.Append(listItem);

            if (isExpanded)
            {
                var children = await GetChildrenAsync(item.Id);
                var group = new HtmlBuilder(HtmlTag.Ul)
                    .WithCssClass("jj-hierarchy-group")
                    .WithAttribute("role", "group");
                await AppendLevelAsync(group, children, path, depth + 1);
                listItem.Append(group);
            }
        }
    }

    private void AppendReadOnlyPath(HtmlBuilder parent, IReadOnlyList<DataItemValue> path, int depth)
    {
        if (depth >= path.Count)
            return;

        var item = path[depth];
        var listItem = GetNodeHtml(item, depth == path.Count - 1, depth < path.Count - 1);
        parent.Append(listItem);
        if (depth + 1 >= path.Count)
            return;

        var group = new HtmlBuilder(HtmlTag.Ul)
            .WithCssClass("jj-hierarchy-group")
            .WithAttribute("role", "group");
        AppendReadOnlyPath(group, path, depth + 1);
        listItem.Append(group);
    }

    private HtmlBuilder GetNodeHtml(DataItemValue item, bool isSelected, bool isExpanded)
    {
        var canExpand = CanExpand(item) || isExpanded;
        var listItem = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("jj-hierarchy-node")
            .WithAttribute("role", "treeitem")
            .WithAttribute("data-id", item.Id)
            .WithAttribute("data-parent-id", item.ParentId ?? string.Empty)
            .WithAttribute("data-can-expand", canExpand ? "true" : "false")
            .WithAttribute("aria-selected", isSelected ? "true" : "false")
            .WithAttributeIf(canExpand, "aria-expanded", isExpanded ? "true" : "false");

        var row = new HtmlBuilder(HtmlTag.Div).WithCssClass("jj-hierarchy-row");
        if (canExpand && !ShowSelectedPathOnly)
        {
            row.Append(HtmlTag.Button, button => button
                .WithCssClass("jj-hierarchy-toggle btn btn-link")
                .WithAttribute("type", "button")
                .WithAttributeIf(!CanInteract, "disabled", "disabled")
                .WithAttribute("aria-label", stringLocalizer[isExpanded ? "Collapse" : "Expand"])
                .Append(HtmlTag.Span, icon => icon
                    .WithCssClass(isExpanded ? "fa-solid fa-chevron-down" : "fa-solid fa-chevron-right")
                    .WithAttribute("aria-hidden", "true")));
        }
        else
        {
            row.Append(HtmlTag.Span, spacer => spacer.WithCssClass("jj-hierarchy-toggle-spacer"));
        }

        row.Append(HtmlTag.Button, button =>
        {
            button.WithCssClass("jj-hierarchy-label btn btn-link")
                .WithCssClassIf(isSelected, "active")
                .WithAttribute("type", "button")
                .WithAttributeIf(!CanInteract, "disabled", "disabled");
            if (DataItem.ShowIcon && item.Icon.HasValue)
                button.Append(new JJIcon(item.Icon.Value, item.IconColor ?? string.Empty).GetHtmlBuilder());
            button.Append(HtmlTag.Span, text => text.WithCssClass("jj-hierarchy-text").AppendText(item.Description ?? item.Id));
        });

        listItem.Append(row);
        return listItem;
    }

    private HtmlBuilder GetActionsHtml(DataItemValue? selectedItem)
    {
        var actions = new HtmlBuilder(HtmlTag.Div).WithCssClass("jj-hierarchy-actions");
        actions.Append(HtmlTag.Button, button => button
            .WithCssClass("btn btn-link btn-sm jj-hierarchy-back")
            .WithAttribute("type", "button")
            .WithAttribute("data-parent-id", selectedItem?.ParentId ?? string.Empty)
            .WithAttributeIf(selectedItem is null || string.IsNullOrEmpty(selectedItem.ParentId), "disabled", "disabled")
            .AppendText(stringLocalizer["Back one level"]));
        if (!IsRequired)
        {
            actions.Append(HtmlTag.Button, button => button
                .WithCssClass("btn btn-link btn-sm jj-hierarchy-clear")
                .WithAttribute("type", "button")
                .WithAttributeIf(string.IsNullOrEmpty(SelectedValue), "disabled", "disabled")
                .AppendText(stringLocalizer["Clear selection"]));
        }

        return actions;
    }

    private string GetQueryString()
    {
        var context = FormStateData.PageState is PageState.Filter
            ? ComponentContext.HierarchyFilter
            : ComponentContext.Hierarchy;
        var routeContext = new RouteContext(ElementName, ParentElementName, context);
        var encryptedRoute = dataProtectionService.ProtectObject(routeContext);
        var query = new StringBuilder();
        query.Append($"&elementName={Uri.EscapeDataString(ElementName ?? string.Empty)}");
        query.Append($"&routeContext={Uri.EscapeDataString(encryptedRoute)}");
        query.Append($"&fieldName={Uri.EscapeDataString(FieldName)}");
        return query.ToString();
    }

    private object ToResult(DataItemValue item) => new
    {
        id = item.Id,
        description = item.Description,
        parentId = item.ParentId,
        canExpand = CanExpand(item),
        iconCssClass = item.Icon?.CssClass,
        iconColor = item.IconColor
    };

    private bool CanExpand(DataItemValue item) =>
        DataItem.DataItemType is not DataItemType.Manual ||
        DataItem.Items?.Any(child => string.Equals(
            child.ParentId?.Trim(), item.Id.Trim(), StringComparison.OrdinalIgnoreCase)) is true;
}
