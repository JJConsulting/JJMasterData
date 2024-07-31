#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridTableBody(JJGridView gridView)
{
    private string Name { get; } = $"{gridView.Name}-table";
    private JJGridView GridView { get; } = gridView;


    public event AsyncEventHandler<ActionEventArgs>? OnRenderActionAsync;
    public event AsyncEventHandler<GridCellEventArgs>? OnRenderCellAsync;
    public event AsyncEventHandler<GridSelectedCellEventArgs>? OnRenderSelectedCellAsync;
    public event AsyncEventHandler<GridRowEventArgs>? OnRenderRowAsync;

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var tbody = new HtmlBuilder(HtmlTag.Tbody);

        tbody.WithAttribute("id", Name);
        tbody.AppendRange(await GetRowsList());

        return tbody;
    }

    private async Task<List<HtmlBuilder>> GetRowsList()
    {
        List<HtmlBuilder> rows = [];
        
        var dataSource = GridView.DataSource;

        for (var i = 0; i < dataSource?.Count; i++)
        {
            rows.Add(await GetRowHtml(dataSource[i], i));
        }

        return rows;
    }

    private async Task<HtmlBuilder> GetRowHtml(Dictionary<string, object?> row, int index)
    {
        var tr = new HtmlBuilder(HtmlTag.Tr);
        var basicActions = GridView.FormElement.Options.GridTableActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x is { IsVisible: true, IsDefaultOption: true });

        tr.WithAttribute("id", $"row{index}");
        var enableGridAction = !GridView.EnableEditMode && (defaultAction != null || GridView.EnableMultiSelect);
        tr.WithCssClassIf(enableGridAction, "tr-hover-action");
        
        tr.AppendRange(await GetTdHtmlList(row, index));

        if (OnRenderRowAsync is not null)
        {
            await OnRenderRowAsync(GridView, new()
            {
                HtmlBuilder = tr,
                RowValues = row
            });
        }

        return tr;
    }

    internal async Task<List<HtmlBuilder>> GetTdHtmlList(Dictionary<string, object?> row, int index)
    {
        var values = await GetValues(row);
        var formStateData = new FormStateData(values, GridView.UserValues, PageState.List);
        var basicActions = GridView.FormElement.Options.GridTableActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x is { IsVisible: true, IsDefaultOption: true });
        var onClickScript = await GetOnClickScript(formStateData, defaultAction);

        var tdList = new List<HtmlBuilder>();
        
        if (GridView.EnableMultiSelect)
        {
            var checkBox = await GetMultiSelectCheckbox(row, index, values);
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("jj-checkbox");

            await td.AppendControlAsync(checkBox);

            if (!GridView.EnableEditMode && onClickScript.Length == 0)
            {
                onClickScript =
                    $"$('#{checkBox.Name}').not(':disabled').prop('checked',!$('#{checkBox.Name}').is(':checked')).change()";
            }

            tdList.Add(td);
        }

        foreach (var visibleFieldHtml in await GetVisibleFieldsHtmlList(row, index, values, onClickScript))
        {
            tdList.Add(visibleFieldHtml);
        }

        foreach (var actionHtml in await GetActionsHtmlListAsync(formStateData))
        {
            tdList.Add(actionHtml);
        }

        return tdList;
    }

    private async Task<List<HtmlBuilder>> GetVisibleFieldsHtmlList(
        Dictionary<string, object?> row, 
        int index,
        Dictionary<string, object?> values,
        string onClickScript)
    {
        List<HtmlBuilder> result = [];
        var formStateData = new FormStateData(values, GridView.UserValues, PageState.List);
        foreach (var field in await GridView.GetVisibleFieldsAsync())
        {
            var formattedValue = string.Empty;
            var stringValue = string.Empty;
            
            if (values.TryGetValue(field.Name, out var value))
            {
                formattedValue = FieldFormattingService.FormatValue(field, value);
                stringValue = value?.ToString() ?? string.Empty;
            }

            var td = new HtmlBuilder(HtmlTag.Td);
            var style = GetTdStyle(field);
            td.WithAttributeIfNotEmpty("style", style);
            td.WithAttributeIfNotEmpty("onclick", onClickScript);

            if (GridView.EnableEditMode && field.DataBehavior != FieldBehavior.ViewOnly)
            {
                td.Append(await GetEditModeFieldHtml(field, formStateData, row, index, formattedValue));
            }
            else
            {
                td.Append(await GetGridFieldHtml(field, formStateData, row, stringValue));
            }

            result.Add(td);
        }

        return result;
    }

    private async Task<HtmlBuilder> GetGridFieldHtml(FormElementField field,
        FormStateData formStateData,
        Dictionary<string, object?> row,
        string stringValue)
    {
        HtmlBuilder cell;

        var isDataIconWithIcon = field.DataItem is { 
            ShowIcon: true, 
            GridBehavior: DataItemGridBehavior.Icon or DataItemGridBehavior.IconWithDescription
        };
        
        if (isDataIconWithIcon)
        {
            cell = await GetDataItemIconCell(field.DataItem!, formStateData, stringValue);
        }
        else if (field.DataFile is not null)
        {
            var controlContext = new ControlContext(formStateData, Name, stringValue);
            var controlFactory = GridView.ComponentFactory.Controls;
            var textFile = controlFactory.Create<JJTextFile>(GridView.FormElement, field,controlContext);
            cell = textFile.GetButtonGroupHtml();
        }
        else if (field.Component is FormComponent.Color && !string.IsNullOrEmpty(stringValue))
        {
            cell = GetIconCell(IconType.Circle, stringValue, stringValue);
        }
        else if (field.Component is FormComponent.Icon && !string.IsNullOrEmpty(stringValue))
        {
            var iconType = IconHelper.GetIconTypeFromField(field, stringValue);

            cell = GetIconCell(iconType, null, iconType.ToString());
        }
        else if (!string.IsNullOrEmpty(field.GridRenderingTemplate))
        {
            var replacedTemplate = ExpressionHelper.ReplaceExpression(field.GridRenderingTemplate!, formStateData.Values, field.EncodeHtml);
            cell = new HtmlBuilder(replacedTemplate);
        }
        else
        {
            var selector = new FormElementFieldSelector(GridView.FormElement, field.Name);
            var gridValue = await GridView.FieldFormattingService.FormatGridValueAsync(selector, formStateData);
            var gridStringValue = gridValue?.Trim() ?? string.Empty;
            cell = new HtmlBuilder(gridStringValue);
        }

        if (OnRenderCellAsync == null) 
            return cell;
        
        var args = new GridCellEventArgs
        {
            Field = field,
            DataRow = row,
            HtmlResult = cell,
            Sender = new JJText(stringValue)
        };

        await OnRenderCellAsync(this, args);

        return args.HtmlResult ?? cell;

    }
    
    private async Task<HtmlBuilder> GetDataItemIconCell(FormElementDataItem dataItem, FormStateData formStateData, string stringValue)
    {
        HtmlBuilder cell;
        var dataQuery = new DataQuery(formStateData, GridView.FormElement.ConnectionId)
        {
            SearchId = stringValue
        };
        var dataItemValues = await GridView.DataItemService.GetValuesAsync(dataItem, dataQuery);
        var dataItemValue = dataItemValues.Find(d => d.Id == stringValue);

        var tooltip = dataItem.GridBehavior is DataItemGridBehavior.Icon ? GridView.StringLocalizer[dataItemValue?.Description ?? string.Empty] : string.Empty;

        if (dataItemValue != null)
        {
            cell = GetIconCell(dataItemValue.Icon, dataItemValue.IconColor ?? string.Empty, tooltip);

            cell.AppendIf(dataItem.GridBehavior is DataItemGridBehavior.IconWithDescription,
                HtmlTag.Span,
                span =>
                {
                    span.AppendText(dataItemValue.Description ?? dataItemValue.Id);
                    span.WithCssClass($"{BootstrapHelper.MarginLeft}-1");
                });
        }
        else
        {
            cell = new HtmlBuilder();
        }

        return cell;
    }

    private static HtmlBuilder GetIconCell(IconType iconType, string? color = null, string? tooltip = null)
    {
        var cell = new HtmlBuilder(HtmlTag.Div);
        var icon = new JJIcon(iconType, color);
        if (tooltip is not null)
        {
            icon.Tooltip = tooltip;
        }
        icon.CssClass += "fa-lg";
        cell.AppendComponent(icon);
        return cell;
    }

    private async Task<HtmlBuilder> GetEditModeFieldHtml(
        FormElementField field,
        FormStateData formStateData,
        Dictionary<string, object?> row,
        int index, 
        string? value)
    {
        var name = GridView.GetFieldName(field.Name, formStateData.Values);
        var hasError = GridView.Errors.ContainsKey(name);

        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClassIf(hasError, BootstrapHelper.HasError);

        var control = GridView.ComponentFactory.Controls.Create(GridView.FormElement, field, formStateData, name, value);
        control.Name = name;
        
        control.Attributes.Add("gridViewRowIndex", index.ToString());
        
        if(field.AutoPostBack)
            control.Attributes.Add("onchange",GridView.Scripts.GetReloadRowScript(field,index));
        
        control.CssClass = field.Name;

        if (OnRenderCellAsync != null)
        {
            var args = new GridCellEventArgs { Field = field, DataRow = row, Sender = control };

            await OnRenderCellAsync(GridView, args);

            if (args.HtmlResult is not null)
            {
                div.Append(args.HtmlResult);
            }
            else
            {
                await div.AppendControlAsync(control);
            }
        }
        else
        {
            await div.AppendControlAsync(control);
        }


        return div;
    }

    public async Task<List<HtmlBuilder>> GetActionsHtmlListAsync(FormStateData formStateData)
    {
        List<HtmlBuilder> result = [];
        var basicActions = GridView.GridTableActions.OrderBy(x => x.Order).ToList();
        var actionsWithoutGroup = basicActions.FindAll(x => x is { IsVisible: true, IsGroup: false });
        var groupedActions = basicActions.FindAll(x => x is { IsVisible: true, IsGroup: true });
        
        foreach (var action in await GetActionsWithoutGroupHtmlAsync(actionsWithoutGroup, formStateData))
        {
            result.Add(action);
        }

        if (groupedActions.Count > 0)
        {
            result.Add(await GetActionsGroupHtmlAsync(groupedActions, formStateData));
        }

        return result;
    }


    private async Task<HtmlBuilder> GetActionsGroupHtmlAsync(List<BasicAction> actions,
        FormStateData formStateData)
    {
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("table-action");

        var btnGroup = GridView.ComponentFactory.Html.LinkButtonGroup.Create();

        var factory = GridView.ComponentFactory.ActionButton;

        foreach (var groupedAction in actions.FindAll(a => a.IsGroup))
        {
            btnGroup.ShowAsButton = groupedAction.ShowAsButton;
            var linkButton = factory.CreateGridTableButton(groupedAction, GridView, formStateData);

            if (OnRenderActionAsync != null)
            {
                var args = new ActionEventArgs(groupedAction, linkButton, formStateData.Values);
                await OnRenderActionAsync(GridView, args);
            }

            btnGroup.Actions.Add(linkButton);
        }

        td.AppendComponent(btnGroup);
        return td;
    }


    private async Task<List<HtmlBuilder>> GetActionsWithoutGroupHtmlAsync(
        IEnumerable<BasicAction> actionsWithoutGroup, FormStateData formStateData)
    {
        var factory = GridView.ComponentFactory.ActionButton;
        List<HtmlBuilder> result = [];
        foreach (var action in actionsWithoutGroup)
        {
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("table-action");
            var link = factory.CreateGridTableButton(action, GridView, formStateData);
            if (OnRenderActionAsync is not null)
            {
                var args = new ActionEventArgs(action, link, formStateData.Values);
                
                await OnRenderActionAsync(GridView, args);

                if (args.HtmlResult != null)
                {
                    td.AppendText(args.HtmlResult);
                    link = null;
                }
            }

            if (link != null)
                td.AppendComponent(link);

            result.Add(td);
        }

        return result;
    }

    private static string GetTdStyle(FormElementField field)
    {
        switch (field.GridAlignment)
        {
            case GridAlignment.Left:
                return "text-align:left";
            case GridAlignment.Center:
                return "text-align:center";
            case GridAlignment.Right:
                return "text-align:right";
        }
        switch (field.Component)
        {
            case FormComponent.ComboBox or FormComponent.RadioButtonGroup:
                if (field.DataItem is
                    {
                        ShowIcon: true,
                        GridBehavior: DataItemGridBehavior.Icon or DataItemGridBehavior.IconWithDescription
                    })
                {
                    return "text-align:center;";
                }

                break;
            case FormComponent.CheckBox:
                return "text-align:center";
            case FormComponent.Icon:
                return "text-align:center";
            default:
                if (field.DataType is FieldType.Float or FieldType.Int)
                {
                    if (!field.IsPk)
                        return "text-align:right";
                }

                break;
        }

        return string.Empty;
    }

    private async Task<JJCheckBox> GetMultiSelectCheckbox(Dictionary<string, object?> row, int index,
        Dictionary<string, object?> values)
    {
        string pkValues = DataHelper.ParsePkValues(GridView.FormElement, values, ';');
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("jj-checkbox");

        var checkBox = new JJCheckBox(GridView.CurrentContext.Request.Form, GridView.StringLocalizer)
        {
            Name = $"jjchk_{index}",
            Value = GridView.EncryptionService.EncryptStringWithUrlEscape(pkValues),
            Text = string.Empty,
            Attributes =
            {
                ["onchange"] = $"GridViewSelectionHelper.selectItem('{GridView.Name}', this);"
            }
        };

        var selectedGridValues = GridView.GetSelectedGridValues();

        checkBox.IsChecked = selectedGridValues.Any(x => x.Any(kvp => kvp.Value.Equals(pkValues)));

        if (OnRenderSelectedCellAsync is not null)
        {
            var args = new GridSelectedCellEventArgs
            {
                DataRow = row,
                CheckBox = checkBox
            };

            await OnRenderSelectedCellAsync(GridView, args);

            if (args.CheckBox != null)
                return checkBox;
        }

        return checkBox;
    }

    private async Task<string> GetOnClickScript(FormStateData formStateData, BasicAction? defaultAction)
    {
        if (GridView.EnableEditMode || defaultAction == null)
            return string.Empty;

        var factory = GridView.ComponentFactory.ActionButton;

        var actionButton = factory.CreateGridTableButton(defaultAction, GridView, formStateData);

        if (OnRenderActionAsync != null)
        {
            var args = new ActionEventArgs(defaultAction, actionButton, formStateData.Values);
            await OnRenderActionAsync(GridView, args);

            if (args.HtmlResult != null)
                actionButton = null;
        }

        if (actionButton is { Visible: true })
        {
            if (!string.IsNullOrEmpty(actionButton.OnClientClick))
                return actionButton.OnClientClick;

            return !string.IsNullOrEmpty(actionButton.UrlAction)
                ? $"window.location.href = '{actionButton.UrlAction}'"
                : string.Empty;
        }

        return string.Empty;
    }

    private async Task<Dictionary<string, object?>> GetValues(Dictionary<string, object?> row)
    {
        if (!GridView.EnableEditMode)
            return row;

        var prefixName = GridView.GetFieldName(string.Empty, row);
        return await GridView.FormValuesService.GetFormValuesWithMergedValuesAsync(GridView.FormElement,
            new FormStateData(row, GridView.UserValues, PageState.List), GridView.AutoReloadFormFields, prefixName);
    }
}