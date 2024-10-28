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
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Events.Args;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridTableBody(JJGridView gridView)
{
    private readonly string _name = $"{gridView.Name}-table";

    public event AsyncEventHandler<ActionEventArgs>? OnRenderActionAsync;
    public event AsyncEventHandler<GridCellEventArgs>? OnRenderCellAsync;
    public event AsyncEventHandler<GridSelectedCellEventArgs>? OnRenderSelectedCellAsync;
    public event AsyncEventHandler<GridRowEventArgs>? OnRenderRowAsync;

    public async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var tbody = new HtmlBuilder(HtmlTag.Tbody);

        tbody.WithAttribute("id", _name);
        tbody.AppendRange(await GetRowsList());

        return tbody;
    }

    private async ValueTask<List<HtmlBuilder>> GetRowsList()
    {
        List<HtmlBuilder> rows = [];
        
        var dataSource = gridView.DataSource;

        for (var i = 0; i < dataSource?.Count; i++)
        {
            rows.Add(await GetRowHtml(dataSource[i], i));
        }

        return rows;
    }

    private async ValueTask<HtmlBuilder> GetRowHtml(Dictionary<string, object?> row, int index)
    {
        var tr = new HtmlBuilder(HtmlTag.Tr);
        var basicActions = gridView.FormElement.Options.GridTableActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x is { IsVisible: true, IsDefaultOption: true });

        tr.WithAttribute("id", $"row{index}");
        var enableGridAction = !gridView.EnableEditMode && (defaultAction != null || gridView.EnableMultiSelect);
        tr.WithCssClassIf(enableGridAction, "tr-hover-action");
        
        tr.AppendRange(await GetTdHtmlList(row, index));

        if (OnRenderRowAsync is not null)
        {
            await OnRenderRowAsync(gridView, new()
            {
                HtmlBuilder = tr,
                RowValues = row
            });
        }

        return tr;
    }

    internal async ValueTask<List<HtmlBuilder>> GetTdHtmlList(Dictionary<string, object?> row, int index)
    {
        var values = await GetValues(row);
        var formStateData = new FormStateData(values, gridView.UserValues, PageState.List);
        var basicActions = gridView.FormElement.Options.GridTableActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x is { IsVisible: true, IsDefaultOption: true });
        var onClickScript = await GetOnClickScript(formStateData, defaultAction);

        var tdList = new List<HtmlBuilder>();
        
        if (gridView.EnableMultiSelect)
        {
            var checkBox = await GetMultiSelectCheckbox(row, index, values);
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("jj-checkbox");

            await td.AppendControlAsync(checkBox);

            if (!gridView.EnableEditMode && onClickScript.Length == 0)
            {
                onClickScript =
                    $"$('#{checkBox.Name}').not(':disabled').prop('checked',!$('#{checkBox.Name}').is(':checked')).change()";
            }

            tdList.Add(td);
        }

        tdList.AddRange(await GetVisibleFieldsHtmlList(row, index, values, onClickScript));
        tdList.AddRange(await GetActionsHtmlListAsync(formStateData));

        return tdList;
    }

    private async ValueTask<List<HtmlBuilder>> GetVisibleFieldsHtmlList(
        Dictionary<string, object?> row, 
        int index,
        Dictionary<string, object?> values,
        string onClickScript)
    {
        List<HtmlBuilder> result = [];
        var formStateData = new FormStateData(values, gridView.UserValues, PageState.List);
        foreach (var field in await gridView.GetVisibleFieldsAsync())
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

            if (gridView.EnableEditMode && field.DataBehavior is not FieldBehavior.ViewOnly)
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

    private async ValueTask<HtmlBuilder> GetGridFieldHtml(FormElementField field,
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
            var controlContext = new ControlContext(formStateData, _name, stringValue);
            var controlFactory = gridView.ComponentFactory.Controls;
            var textFile = controlFactory.Create<JJTextFile>(gridView.FormElement, field,controlContext);
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
            var replacedTemplate = await gridView.HtmlTemplateService.RenderTemplate(field.GridRenderingTemplate!, formStateData.Values);
            cell = new HtmlBuilder(replacedTemplate);
        }
        else
        {
            var selector = new FormElementFieldSelector(gridView.FormElement, field.Name);
            var gridValue = await gridView.FieldFormattingService.FormatGridValueAsync(selector, formStateData);
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
        var dataQuery = new DataQuery(formStateData, gridView.FormElement.ConnectionId)
        {
            SearchId = stringValue
        };
        var dataItemValues = await gridView.DataItemService.GetValuesAsync(dataItem, dataQuery);
        var dataItemValue = dataItemValues.Find(d => d.Id == stringValue);

        var tooltip = dataItem.GridBehavior is DataItemGridBehavior.Icon ? gridView.StringLocalizer[dataItemValue?.Description ?? string.Empty] : string.Empty;

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

    private async ValueTask<HtmlBuilder> GetEditModeFieldHtml(
        FormElementField field,
        FormStateData formStateData,
        Dictionary<string, object?> row,
        int index, 
        string? value)
    {
        var name = gridView.GetFieldName(field.Name, formStateData.Values);
        var hasError = gridView.Errors.ContainsKey(name);

        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClassIf(hasError, BootstrapHelper.HasError);

        var control = gridView.ComponentFactory.Controls.Create(gridView.FormElement, field, formStateData, name, value);
        control.Name = name;
        
        control.Attributes.Add("gridViewRowIndex", index.ToString());
        
        if(field.AutoPostBack)
            control.Attributes.Add("onchange", gridView.Scripts.GetReloadRowScript(field,index));
        
        control.CssClass = field.Name;

        if (OnRenderCellAsync != null)
        {
            var args = new GridCellEventArgs { Field = field, DataRow = row, Sender = control };

            await OnRenderCellAsync(gridView, args);

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

    public async ValueTask<List<HtmlBuilder>> GetActionsHtmlListAsync(FormStateData formStateData)
    {
        List<HtmlBuilder> result = [];
        var basicActions = gridView.GridTableActions.OrderBy(x => x.Order).ToList();
        var actionsWithoutGroup = basicActions.FindAll(x => x is { IsVisible: true, IsGroup: false });
        var groupedActions = basicActions.FindAll(x => x is { IsVisible: true, IsGroup: true });
        
        result.AddRange(await GetActionsWithoutGroupHtmlAsync(actionsWithoutGroup, formStateData));

        if (groupedActions.Count > 0)
        {
            result.Add(await GetActionsGroupHtmlAsync(groupedActions, formStateData));
        }

        return result;
    }
    
    private async ValueTask<HtmlBuilder> GetActionsGroupHtmlAsync(List<BasicAction> actions,
        FormStateData formStateData)
    {
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("table-action");

        var btnGroup = gridView.ComponentFactory.Html.LinkButtonGroup.Create();

        var factory = gridView.ComponentFactory.ActionButton;

        foreach (var groupedAction in actions.FindAll(a => a.IsGroup))
        {
            btnGroup.ShowAsButton = groupedAction.ShowAsButton;
            var linkButton = factory.CreateGridTableButton(groupedAction, gridView, formStateData);

            if (OnRenderActionAsync != null)
            {
                var args = new ActionEventArgs(groupedAction, linkButton, formStateData.Values);
                await OnRenderActionAsync(gridView, args);
            }

            btnGroup.Actions.Add(linkButton);
        }

        td.AppendComponent(btnGroup);
        return td;
    }
    
    private async ValueTask<List<HtmlBuilder>> GetActionsWithoutGroupHtmlAsync(
        List<BasicAction> actionsWithoutGroup, FormStateData formStateData)
    {
        var factory = gridView.ComponentFactory.ActionButton;
        List<HtmlBuilder> result = [];
        foreach (var action in actionsWithoutGroup)
        {
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("table-action");
            var link = factory.CreateGridTableButton(action, gridView, formStateData);
            if (OnRenderActionAsync is not null)
            {
                var args = new ActionEventArgs(action, link, formStateData.Values);
                
                await OnRenderActionAsync(gridView, args);

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

    private async ValueTask<JJCheckBox> GetMultiSelectCheckbox(Dictionary<string, object?> row, int index,
        Dictionary<string, object?> values)
    {
        string pkValues = DataHelper.ParsePkValues(gridView.FormElement, values, ';');
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("jj-checkbox");

        var checkBox = new JJCheckBox(gridView.CurrentContext.Request.Form, gridView.StringLocalizer)
        {
            Name = $"jjchk_{index}",
            Value = gridView.EncryptionService.EncryptStringWithUrlEscape(pkValues),
            Text = string.Empty,
            Attributes =
            {
                ["onchange"] = $"GridViewSelectionHelper.selectItem('{gridView.Name}', this);"
            }
        };

        var selectedGridValues = gridView.GetSelectedGridValues();

        checkBox.IsChecked = selectedGridValues.Any(x => x.Any(kvp => kvp.Value.Equals(pkValues)));

        if (OnRenderSelectedCellAsync is not null)
        {
            var args = new GridSelectedCellEventArgs
            {
                DataRow = row,
                CheckBox = checkBox
            };

            await OnRenderSelectedCellAsync(gridView, args);

            if (args.CheckBox != null)
                return checkBox;
        }

        return checkBox;
    }

    private async ValueTask<string> GetOnClickScript(FormStateData formStateData, BasicAction? defaultAction)
    {
        if (gridView.EnableEditMode || defaultAction == null)
            return string.Empty;

        var factory = gridView.ComponentFactory.ActionButton;

        var actionButton = factory.CreateGridTableButton(defaultAction, gridView, formStateData);

        if (OnRenderActionAsync != null)
        {
            var args = new ActionEventArgs(defaultAction, actionButton, formStateData.Values);
            await OnRenderActionAsync(gridView, args);

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

    private async ValueTask<Dictionary<string, object?>> GetValues(Dictionary<string, object?> row)
    {
        if (!gridView.EnableEditMode)
            return row;

        var autoReloadFormFields = gridView is
                                   {
                                       AutoReloadFormFields: true, EnableEditMode: false
                                   } ||
                                   gridView.ComponentContext is ComponentContext.GridViewRow;
        
        var prefixName = gridView.GetFieldName(string.Empty, row);
        return await gridView.FormValuesService.GetFormValuesWithMergedValuesAsync(gridView.FormElement,
            new FormStateData(row, gridView.UserValues, PageState.List), autoReloadFormFields, prefixName);
    }
}