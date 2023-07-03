using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class GridTableBody
{
    private string Name => $"table_{GridView.Name}";
    private JJGridView GridView { get; }
    public EventHandler<ActionEventArgs> OnRenderAction { get; set; }
    public EventHandler<GridCellEventArgs> OnRenderCell { get; set; }
    public EventHandler<GridSelectedCellEventArgs> OnRenderSelectedCell { get; set; }

    public GridTableBody(JJGridView gridView)
    {
        GridView = gridView;
    }

    public HtmlBuilder GetHtmlElement()
    {
        var tbody = new HtmlBuilder(HtmlTag.Tbody);

        tbody.WithAttribute("id", Name);
        tbody.AppendRange(GetRowsList());

        return tbody;
    }

    private IEnumerable<HtmlBuilder> GetRowsList()
    {
        var rows = GridView.DataSource.Rows;
        
        for (int i = 0; i < rows.Count; i++)
        {
            yield return GetRowHtml(rows[i],i);
        }
    }

    internal HtmlBuilder GetRowHtml(DataRow row, int index)
    {
        var html = new HtmlBuilder(HtmlTag.Tr);
        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x.IsVisible && x.IsDefaultOption);

        html.WithAttribute("id", $"row{index}");
        bool enableGridAction = !GridView.EnableEditMode && (defaultAction != null || GridView.EnableMultSelect);
        html.WithCssClassIf(enableGridAction, "jjgrid-action");

        html.AppendRange(GetTdHtmlList(row,index));
        
        return html;
    }
    
    internal IEnumerable<HtmlBuilder> GetTdHtmlList(DataRow row, int index)
    {
        var values = GetValues(row);

        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x.IsVisible && x.IsDefaultOption);

        var html = new List<HtmlBuilder>();
        

        string onClickScript = GetOnClickScript(values, defaultAction);
        
        if (GridView.EnableMultSelect)
        {
            var checkBox = GetMultiSelect(row, index, values);
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("jjselect");
            td.AppendElement(checkBox);
            html.Add(td);

            if (!GridView.EnableEditMode && onClickScript == string.Empty)
            {
                onClickScript =
                    $"$('#{checkBox.Name}').not(':disabled').prop('checked',!$('#{checkBox.Name}').is(':checked')).change()";
            }
        }

        html.AddRange(GetVisibleFieldsHtmlList(row, index, values, onClickScript));
        html.AddRange(GetActionsHtmlList(values));

        return html;
    }

    [Obsolete("Must be async")]
    private IEnumerable<HtmlBuilder> GetVisibleFieldsHtmlList(DataRow row, int index, IDictionary<string,dynamic>values, string onClickScript)
    {
        foreach (var field in GridView.VisibleFields)
        {
            string value = string.Empty;
            if (values.ContainsKey(field.Name))
            {
                value = GridView.FieldFormattingService.FormatGridValue(field, values, GridView.UserValues).GetAwaiter().GetResult();
            }

            var td = new HtmlBuilder(HtmlTag.Td);
            string style = GetTdStyle(field);
            td.WithAttributeIf(!string.IsNullOrEmpty(style), "style", style);
            td.WithAttribute("onclick", onClickScript);

            if (GridView.EnableEditMode && field.DataBehavior != FieldBehavior.ViewOnly)
            {
                td.AppendElement(GetEditModeFieldHtml(field, row, index, values, value));
            }
            else
            {
                var renderCell = OnRenderCell;
                if (renderCell != null)
                {
                    var args = new GridCellEventArgs
                    {
                        Field = field,
                        DataRow = row,
                        Sender = new JJText(value)
                    };
                    OnRenderCell.Invoke(GridView, args);
                    td.AppendText(args.HtmlResult);
                }
                else
                {
                    if (field.Component == FormComponent.File)
                    {
                        var upload = (JJTextFile)GridView.FieldManager.GetField(field, PageState.List, values,GridView.UserValues, value);
                        td.AppendElement(upload.GetButtonGroupHtml());
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

    private HtmlBuilder GetEditModeFieldHtml(FormElementField field, DataRow row, int index, IDictionary<string,dynamic>values,
        string value)
    {
        string name = GridView.GetFieldName(field.Name, values);
        bool hasError = GridView.Errors?.ContainsKey(name) ?? false;

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

        var baseField = GridView.FieldManager.GetField(field, PageState.List, values,GridView.UserValues, value);
        baseField.Name = name;
        baseField.Attributes.Add("nRowId", index);
        baseField.CssClass = field.Name;

        var renderCell = OnRenderCell;
        if (renderCell != null)
        {
            var args = new GridCellEventArgs { Field = field, DataRow = row, Sender = baseField };

            OnRenderCell.Invoke(GridView, args);
            div.AppendText(args.HtmlResult);
        }
        else
        {
            div.AppendElement(baseField);
        }

        return div;
    }

    public IEnumerable<HtmlBuilder> GetActionsHtmlList(IDictionary<string,dynamic>values)
    {
        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var actionsWithoutGroup = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var groupedActions = basicActions.FindAll(x => x.IsVisible && x.IsGroup);
        
        foreach (var action in GetActionsWithoutGroupHtml(actionsWithoutGroup, values))
        {
            yield return action;
        }

        if (groupedActions.Count > 0)
        {
            var context = new ActionContext(values, PageState.List, ActionSource.GridTable, OnRenderAction);
            yield return GridView.ActionManager.GetGroupedActionsHtml(groupedActions, context);
        }
        
    }

    private IEnumerable<HtmlBuilder> GetActionsWithoutGroupHtml(IEnumerable<BasicAction> actionsWithoutGroup,
        IDictionary<string,dynamic>values)
    {
        foreach (var action in actionsWithoutGroup)
        {
            var td = new HtmlBuilder(HtmlTag.Td);
            td.WithCssClass("table-action");

            var link = GridView.ActionManager.GetLinkGrid(action, values);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, values);
                onRender.Invoke(GridView, args);
                if (args.HtmlResult != null)
                {
                    td.AppendText(args.HtmlResult);
                    link = null;
                }
            }

            if (link != null)
                td.AppendElement(link);
            
            yield return td;
        }
    }

    private static string GetTdStyle(FormElementField field)
    {
        switch (field.Component)
        {
            case FormComponent.ComboBox:
            {
                if (field.DataItem is { ShowImageLegend: true, ReplaceTextOnGrid: false })
                {
                    return "text-align:center;";
                }

                break;
            }
            case FormComponent.CheckBox:
                return "text-align:center";
            default:
            {
                if (field.DataType is FieldType.Float or FieldType.Int)
                {
                    if (!field.IsPk)
                        return "text-align:right";
                }

                break;
            }
        }

        return string.Empty;
    }

    private JJCheckBox GetMultiSelect(DataRow row, int index, IDictionary<string,dynamic>values)
    {
        string pkValues = DataHelper.ParsePkValues(GridView.FormElement, values, ';');
        var td = new HtmlBuilder(HtmlTag.Td);
        td.WithCssClass("jjselect");

        var checkBox = new JJCheckBox
        {
            Name = "jjchk_" + index,
            Value = Cript.Cript64(pkValues),
            Text = string.Empty,
            IsChecked = GridView.GetSelectedGridValues().Any(x => x.ContainsKey(pkValues))
        };

        if (OnRenderSelectedCell != null)
        {
            var args = new GridSelectedCellEventArgs
            {
                DataRow = row,
                CheckBox = checkBox
            };
            OnRenderSelectedCell.Invoke(GridView, args);
            if (args.CheckBox != null)
                return checkBox;
        }

        return checkBox;  
    }

    private string GetOnClickScript(IDictionary<string,dynamic>values, BasicAction defaultAction)
    {
        if (GridView.EnableEditMode || defaultAction == null) 
            return string.Empty;

        var linkDefaultAction = GridView.ActionManager.GetLinkGrid(defaultAction, values);

        if (OnRenderAction != null)
        {
            var args = new ActionEventArgs(defaultAction, linkDefaultAction, values);
            OnRenderAction.Invoke(GridView, args);

            if (args.HtmlResult != null)
            {
                linkDefaultAction = null;
            }
        }

        if (linkDefaultAction is { Visible: true })
        {
            if (!string.IsNullOrEmpty(linkDefaultAction.OnClientClick))
                return linkDefaultAction.OnClientClick;

            return !string.IsNullOrEmpty(linkDefaultAction.UrlAction)
                ? $"window.location.href = '{linkDefaultAction.UrlAction}'"
                : string.Empty;
        }

        return string.Empty;
    }

    private IDictionary<string,dynamic>GetValues(DataRow row)
    {
        var values = new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase);
        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            values.Add(row.Table.Columns[i].ColumnName, row[i]);
        }

        if (!GridView.EnableEditMode) 
            return values;

        var prefixName = GridView.GetFieldName(string.Empty, values);
        return GridView.FormValuesService.GetFormValuesWithMergedValues(GridView.FormElement,PageState.List, GridView.AutoReloadFormFields, prefixName).GetAwaiter().GetResult();
    }
}