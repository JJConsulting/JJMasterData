using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Tasks;

namespace JJMasterData.Core.Web.Components;

internal class GridTableBody
{
    private string Name => $"table_{GridView.Name}";
    private JJGridView GridView { get; }
    public event EventHandler<ActionEventArgs> OnRenderAction;
    public event EventHandler<GridCellEventArgs> OnRenderCell;
    public event EventHandler<GridSelectedCellEventArgs> OnRenderSelectedCell;

    
    public event AsyncEventHandler<ActionEventArgs> OnRenderActionAsync;
    public event AsyncEventHandler<GridCellEventArgs> OnRenderCellAsync;
    public event AsyncEventHandler<GridSelectedCellEventArgs> OnRenderSelectedCellAsync;
    
    public GridTableBody(JJGridView gridView)
    {
        GridView = gridView;
    }

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var tbody = new HtmlBuilder(HtmlTag.Tbody);

        tbody.WithAttribute("id", Name);
        await tbody.AppendRangeAsync(GetRowsList());

        return tbody;
    }

    private async IAsyncEnumerable<HtmlBuilder> GetRowsList()
    {
        var rows = GridView.DataSource;

        for (int i = 0; i < rows?.Count; i++)
        {
            yield return await GetRowHtml(rows[i], i);
        }
    }

    internal async Task<HtmlBuilder> GetRowHtml(IDictionary<string,object> row, int index)
    {
        var html = new HtmlBuilder(HtmlTag.Tr);
        var basicActions = GridView.FormElement.Options.GridTableActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x.IsVisible && x.IsDefaultOption);

        html.WithAttribute("id", $"row{index}");
        bool enableGridAction = !GridView.EnableEditMode && (defaultAction != null || GridView.EnableMultiSelect);
        html.WithCssClassIf(enableGridAction, "jjgrid-action");

        await html.AppendRangeAsync(GetTdHtmlList(row, index));

        return html;
    }

    internal async IAsyncEnumerable<HtmlBuilder> GetTdHtmlList(IDictionary<string,object> row, int index)
    {
        var values = await GetValues(row);
        var formData = new FormStateData(values, GridView.UserValues, PageState.List);
        var basicActions = GridView.FormElement.Options.GridTableActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x.IsVisible && x.IsDefaultOption);

        string onClickScript = await GetOnClickScript(formData, defaultAction);

        if (GridView.EnableMultiSelect)
        {
            var checkBox = await GetMultiSelect(row, index, values);
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("jjselect");
            await td.AppendControlAsync(checkBox);

            if (!GridView.EnableEditMode && onClickScript == string.Empty)
            {
                onClickScript =
                    $"$('#{checkBox.Name}').not(':disabled').prop('checked',!$('#{checkBox.Name}').is(':checked')).change()";
            }

            yield return td;
        }

        await foreach (var visibleFieldHtml in GetVisibleFieldsHtmlList(row, index, values, onClickScript))
        {
            yield return visibleFieldHtml;
        }

        await foreach (var actionHtml in GetActionsHtmlListAsync(formData))
        {
            yield return actionHtml;
        }
    }
    
    private async IAsyncEnumerable<HtmlBuilder> GetVisibleFieldsHtmlList(IDictionary<string,object> row, int index, IDictionary<string, object> values, string onClickScript)
    {
        await foreach (var field in GridView.GetVisibleFieldsAsync())
        {
            string value = string.Empty;
            if (values.ContainsKey(field.Name))
            {
                value = await GridView.FieldsService.FormatGridValueAsync(field, values, GridView.UserValues);
            }

            var td = new HtmlBuilder(HtmlTag.Td);
            string style = GetTdStyle(field);
            td.WithAttributeIf(string.IsNullOrEmpty(style), "style", style!);
            td.WithAttributeIfNotEmpty( "style", style);
            td.WithAttribute("onclick", onClickScript);

            if (GridView.EnableEditMode && field.DataBehavior != FieldBehavior.ViewOnly)
            {
                td.Append(await GetEditModeFieldHtml(field, row, index, values, value));
            }
            else
            {
                if (OnRenderCell != null || OnRenderCellAsync != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = field,
                        DataRow = row,
                        Sender = new JJText(value)
                    };
                    OnRenderCell?.Invoke(this,args);

                    if (OnRenderCellAsync is not null)
                    {
                        await OnRenderCellAsync(this, args);
                    }
                    
                    if (args.HtmlResult != null)
                    {
                        td.AppendText(args.HtmlResult);
                    }
                    else
                    {
                        td.AppendText(value.Trim());
                    }
                }
                else
                {
                    if (field.Component == FormComponent.File)
                    {
                        var upload = (JJTextFile)await GridView.ComponentFactory.Controls.CreateAsync(GridView.FormElement, field, new(values, GridView.UserValues, PageState.List),  value);
                        td.Append(upload.GetButtonGroupHtml());
                    }
                    else
                    {
                        td.AppendText(value.Trim());
                    }
                }
            }

            yield return td;
        }
    }

    private async Task<HtmlBuilder> GetEditModeFieldHtml(FormElementField field, IDictionary<string,object> row, int index, IDictionary<string, object> values,
        string value)
    {
        string name = GridView.GetFieldName(field.Name, values);
        bool hasError = GridView.Errors.ContainsKey(name);

        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClassIf(hasError, BootstrapHelper.HasError);
        if (field.Component
                is FormComponent.ComboBox
                or FormComponent.CheckBox
                or FormComponent.Search
                or FormComponent.Number
            && values.TryGetValue(field.Name, out var value1))
        {
            value = value1.ToString();
        }

        var control = await GridView.ComponentFactory.Controls.CreateAsync(GridView.FormElement, field, new(values, GridView.UserValues, PageState.List), value);
        control.Name = name;
        control.Attributes.Add("nRowId", index.ToString());
        control.CssClass = field.Name;

        var renderCell = OnRenderCell;
        if (renderCell != null)
        {
            var args = new GridCellEventArgs { Field = field, DataRow = row, Sender = control };

            OnRenderCell?.Invoke(GridView, args);
            div.AppendText(args.HtmlResult);
        }
        else
        {
            await div.AppendControlAsync(control);
        }
          

        return div;
    }

    public async IAsyncEnumerable<HtmlBuilder> GetActionsHtmlListAsync(FormStateData formStateData)
    {
        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var actionsWithoutGroup = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var groupedActions = basicActions.FindAll(x => x.IsVisible && x.IsGroup);
        await foreach (var action in GetActionsWithoutGroupHtmlAsync(actionsWithoutGroup, formStateData))
        {
            yield return action;
        }

        if (groupedActions.Count > 0)
        {
            yield return await GetActionsGroupHtmlAsync(groupedActions, formStateData);
        }
    }


    private async Task<HtmlBuilder> GetActionsGroupHtmlAsync(IEnumerable<BasicAction> actions, FormStateData formStateData)
    {
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("table-action");

        var btnGroup = new JJLinkButtonGroup();
        
        var factory = GridView.ComponentFactory.LinkButton;
        
        foreach (var groupedAction in actions.Where(a => a.IsGroup).ToList())
        {
            btnGroup.ShowAsButton = groupedAction.ShowAsButton;
            var linkButton = await factory.CreateGridTableButtonAsync(groupedAction, GridView, formStateData);

            if (OnRenderAction != null || OnRenderActionAsync != null)
            {
                var args = new ActionEventArgs(groupedAction, linkButton, formStateData.FormValues);
                
                OnRenderAction?.Invoke(this,args);

                if (OnRenderActionAsync is not null)
                {
                    await OnRenderActionAsync(this, args);
                }
            }
            
            btnGroup.Actions.Add(linkButton);
        }

        td.AppendComponent(btnGroup);
        return td;
    }


    private async IAsyncEnumerable<HtmlBuilder> GetActionsWithoutGroupHtmlAsync(IEnumerable<BasicAction> actionsWithoutGroup, FormStateData formStateData)
    {
        var factory = GridView.ComponentFactory.LinkButton;
        foreach (var action in actionsWithoutGroup)
        {
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("table-action");
            var link =  await factory.CreateGridTableButtonAsync(action, GridView, formStateData);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, formStateData.FormValues);
                onRender.Invoke(GridView, args);
                if (args.HtmlResult != null)
                {
                    td.AppendText(args.HtmlResult);
                    link = null;
                }
            }

            if (link != null)
                td.AppendComponent(link);

            yield return td;
        }
    }

    private static string GetTdStyle(FormElementField field)
    {
        switch (field.Component)
        {
            case FormComponent.ComboBox:
                if (field.DataItem is { ShowImageLegend: true, ReplaceTextOnGrid: false })
                {
                    return "text-align:center;";
                }

                break;
            case FormComponent.CheckBox:
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

    private async Task<JJCheckBox> GetMultiSelect(IDictionary<string,object> row, int index, IDictionary<string, object> values)
    {
        string pkValues = DataHelper.ParsePkValues(GridView.FormElement, values, ';');
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("jjselect");

        var checkBox = new JJCheckBox(GridView.CurrentContext.Request)
        {
            Name = "jjchk_" + index,
            Value = GridView.EncryptionService.EncryptStringWithUrlEscape(pkValues),
            Text = string.Empty
        };

        var selectedGridValues = GridView.GetSelectedGridValues();
        
        checkBox.IsChecked = selectedGridValues.Any(x => x.Any(kvp => kvp.Value.Equals(pkValues)));

        if (OnRenderSelectedCell is not null || OnRenderSelectedCellAsync is not  null)
        {
            var args = new GridSelectedCellEventArgs
            {
                DataRow = row,
                CheckBox = checkBox
            };
            OnRenderSelectedCell?.Invoke(GridView, args);

            if (OnRenderSelectedCellAsync is not null)
            {
                await OnRenderSelectedCellAsync(this, args);
            }
            
            if (args.CheckBox != null)
                return checkBox;
        }

        return checkBox;
    }

    private async Task<string> GetOnClickScript(FormStateData formStateData, BasicAction defaultAction)
    {
        if (GridView.EnableEditMode || defaultAction == null)
            return string.Empty;

        var factory = GridView.ComponentFactory.LinkButton;
        
        var actionButton =  await factory.CreateGridTableButtonAsync(defaultAction, GridView, formStateData);

        if (OnRenderAction != null)
        {
            var args = new ActionEventArgs(defaultAction, actionButton, formStateData.FormValues);
            OnRenderAction.Invoke(GridView, args);

            if (args.HtmlResult != null)
            {
                actionButton = null;
            }
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

    private async Task<IDictionary<string, object>> GetValues(IDictionary<string,object> row)
    {
        if (!GridView.EnableEditMode)
            return row;

        var prefixName = GridView.GetFieldName(string.Empty, row);
        return await GridView.FormValuesService.GetFormValuesWithMergedValuesAsync(GridView.FormElement, PageState.List,row, GridView.AutoReloadFormFields, prefixName);
    }
}