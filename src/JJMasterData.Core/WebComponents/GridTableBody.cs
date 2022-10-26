using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Linq;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents;

internal class GridTableBody
{
    private string Name => $"table_{GridView.Name}";
    private JJGridView GridView { get; }

    //TODO: When C# 11 releases, add the required keyword
    public EventHandler<ActionEventArgs> OnRenderAction { get; set; }
    public EventHandler<GridCellEventArgs> OnRenderCell { get; set; }
    public EventHandler<GridSelectedCellEventArgs> OnRenderSelectedCell { get; set; }

    public GridTableBody(JJGridView gridView)
    {
        GridView = gridView;
    }

    public HtmlElement GetHtmlElement()
    {
        var tbody = new HtmlElement(HtmlTag.Tbody);

        tbody.WithAttribute("id", Name);
        tbody.AppendRange(GetRowsList());

        return tbody;
    }

    private IList<HtmlElement> GetRowsList(bool isAjax = false)
    {
        var rows = GridView.DataSource.Rows;
        var trList = new List<HtmlElement>();

        for (int i = 0; i < rows.Count; i++)
        {
            trList.Add(GetRowHtmlElement(rows[i], i, false));
        }

        return trList;
    }

    internal HtmlElement GetRowHtmlElement(DataRow row, int index, bool isAjax)
    {
        var values = GetValues(row);

        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var defaultAction = basicActions.Find(x => x.IsVisible && x.IsDefaultOption);

        var html = new HtmlElement(!isAjax ? HtmlTag.Tr : HtmlTag.Div);

        if (!isAjax)
        {
            html.WithAttribute("id", $"row{index}");
            bool enableGridAction = !GridView.EnableEditMode && (defaultAction != null || GridView.EnableMultSelect);
            html.WithCssClassIf(enableGridAction, "jjgrid-action");
        }

        string onClickScript = GetOnClickScript(values, defaultAction);

        html.AppendElementIf(GridView.EnableMultSelect, () => GetMultiSelectRow(row, index, values, ref onClickScript));

        html.AppendRange(GetVisibleFieldsHtmlList(row, index, values, onClickScript));

        return html;
    }

    private IList<HtmlElement> GetVisibleFieldsHtmlList(DataRow row, int index, Hashtable values, string onClickScript)
    {
        var htmlList = new List<HtmlElement>();
        foreach (var field in GridView.VisibleFields)
        {
            string value = string.Empty;
            if (values.Contains(field.Name))
            {
                value = GridView.FieldManager.ParseVal(values, field);
            }

            var td = new HtmlElement(HtmlTag.Td);
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
                    OnRenderCell.Invoke(this, args);
                    td.AppendText(args.HtmlResult);
                }
                else
                {
                    if (field.Component == FormComponent.File)
                    {
                        var upload = (JJTextFile)GridView.FieldManager.GetField(field, PageState.List, values, value);
                        td.AppendElement(upload.GetButtonGroupHtml());
                    }
                    else
                    {
                        td.AppendText(value.Trim());
                    }
                }
            }

            htmlList.Add(td);
        }

        htmlList.AddRange(GetActionsHtmlList(values));
        return htmlList;
    }

    private HtmlElement GetEditModeFieldHtml(FormElementField field, DataRow row, int index, Hashtable values,
        string value)
    {
        string name = GridView.GetFieldName(field.Name, values);
        bool hasError = GridView.Errors?.ContainsKey(name) ?? false;

        var div = new HtmlElement(HtmlTag.Div);

        div.WithCssClassIf(hasError, BootstrapHelper.HasError);
        if ((field.Component == FormComponent.ComboBox | field.Component == FormComponent.CheckBox |
             field.Component == FormComponent.Search) & values.Contains(field.Name))
        {
            value = values[field.Name].ToString();
        }

        var baseField = GridView.FieldManager.GetField(field, PageState.List, values, value);
        baseField.Name = name;
        baseField.Attributes.Add("nRowId", index);
        baseField.CssClass = field.Name;

        var renderCell = OnRenderCell;
        if (renderCell != null)
        {
            var args = new GridCellEventArgs { Field = field, DataRow = row, Sender = baseField };

            OnRenderCell.Invoke(this, args);
            div.AppendText(args.HtmlResult);
        }
        else
        {
            div.AppendElement(baseField);
        }

        return div;
    }

    public IEnumerable<HtmlElement> GetActionsHtmlList(Hashtable values)
    {
        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var actionsWithoutGroup = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var groupedActions = basicActions.FindAll(x => x.IsVisible && x.IsGroup);
        var htmlList = new List<HtmlElement>();

        htmlList.AddRange(GetActionsWithoutGroupHtml(actionsWithoutGroup, values));

        if (groupedActions.Count > 0)
        {
            htmlList.Add(GetGroupedActionsHtml(groupedActions, values));
        }

        return htmlList;
    }

    private IEnumerable<HtmlElement> GetActionsWithoutGroupHtml(IEnumerable<BasicAction> actionsWithoutGroup,
        Hashtable values)
    {
        var tdList = new List<HtmlElement>();
        foreach (var action in actionsWithoutGroup)
        {
            var td = new HtmlElement(HtmlTag.Td);
            td.WithCssClass("table-action");

            var link = GridView.ActionManager.GetLinkGrid(action, values);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, values);
                onRender.Invoke(this, args);
                if (args.HtmlResult != null)
                {
                    td.AppendText(args.HtmlResult);
                    link = null;
                }
            }

            if (link != null)
                td.AppendElement(link);
            tdList.Add(td);
        }

        return tdList;
    }

    private HtmlElement GetGroupedActionsHtml(List<BasicAction> actionsWithGroup, Hashtable values)
    {
        var td = new HtmlElement(HtmlTag.Td)
            .WithCssClass("table-action")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.InputGroupBtn);
                div.AppendElement(BootstrapHelper.Version == 3 ? HtmlTag.Button : HtmlTag.A,
                    element =>
                    {
                        element.WithAttribute("type", "button");
                        element.WithCssClass("btn-link dropdown-toggle");
                        element.WithAttribute(BootstrapHelper.DataToggle, "dropdown");
                        element.WithAttribute("aria-haspopup", "true");
                        element.WithAttribute("aria-expanded", "false");
                        element.AppendElement(HtmlTag.Span, span =>
                        {
                            span.WithCssClass("caret");
                            span.WithToolTip(Translate.Key("More Options"));
                        });
                    });
                div.AppendElement(HtmlTag.Ul, ul =>
                {
                    ul.WithCssClass("dropdown-menu dropdown-menu-right");
                    foreach (var action in actionsWithGroup)
                    {
                        var link = GridView.ActionManager.GetLinkGrid(action, values);
                        var onRender = OnRenderAction;
                        if (onRender != null)
                        {
                            var args = new ActionEventArgs(action, link, values);
                            onRender.Invoke(this, args);
                        }

                        if (link is { Visible: true })
                        {
                            ul.AppendElementIf(action.DividerLine, GetDividerHtml);
                            ul.AppendElement(HtmlTag.Li, li =>
                            {
                                li.WithCssClass("dropdown-item");
                                li.AppendElement(link);
                            });
                        }
                    }
                });
            });
        return td;
    }

    private static HtmlElement GetDividerHtml()
    {
        var li = new HtmlElement(HtmlTag.Li)
            .WithCssClass("separator")
            .WithCssClass("divider");

        return li;
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

    private HtmlElement GetMultiSelectRow(DataRow row, int index, Hashtable values, ref string onClickScript)
    {
        string pkValues = GridView.GetPkValues(values);

        var td = new HtmlElement(HtmlTag.Td);
        td.WithCssClass("jjselect");

        var checkBox = new JJCheckBox
        {
            Name = "jjchk_" + index,
            Value = Cript.Cript64(pkValues),
            IsChecked = GridView.GetSelectedGridValues().Any(x => x.ContainsValue(pkValues))
        };

        if (OnRenderSelectedCell != null)
        {
            var args = new GridSelectedCellEventArgs
            {
                DataRow = row,
                CheckBox = checkBox
            };
            OnRenderSelectedCell.Invoke(this, args);
            if (args.CheckBox != null)
                td.AppendElement(checkBox);
        }
        else
        {
            td.AppendElement(checkBox);
        }

        if (onClickScript == string.Empty)
        {
            onClickScript =
                $"$('#{checkBox.Name}').not(':disabled').prop('checked',!$('#{checkBox.Name}').is(':checked')).change()";
        }

        return td;
    }

    private string GetOnClickScript(Hashtable values, BasicAction defaultAction)
    {
        if (GridView.EnableEditMode || defaultAction == null) return string.Empty;

        var linkDefaultAction = GridView.ActionManager.GetLinkGrid(defaultAction, values);

        if (OnRenderAction != null)
        {
            var args = new ActionEventArgs(defaultAction, linkDefaultAction, values);
            OnRenderAction.Invoke(this, args);

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

    private Hashtable GetValues(DataRow row)
    {
        var values = new Hashtable();

        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            values.Add(row.Table.Columns[i].ColumnName, row[i]);
        }

        if (!GridView.EnableEditMode) return values;

        string prefixName = GridView.GetFieldName(string.Empty, values);

        values = GridView.FieldManager.GetFormValues(prefixName, GridView.FormElement, PageState.List, values,
            GridView.AutoReloadFormFields);

        return values;
    }
}