using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Linq;
using System.Text;
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

        //TODO:GetHtmlAction return a true element.
        htmlList.Add(new(GetHtmlAction(values)));
        return htmlList;
    }

    HtmlElement GetEditModeFieldHtml(FormElementField field, DataRow row, int index, Hashtable values, string value)
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

    public string GetHtmlAction(Hashtable values)
    {
        //Actions
        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var listAction = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var listActionGroup = basicActions.FindAll(x => x.IsVisible && x.IsGroup);

        var html = new StringBuilder();
        foreach (var action in listAction)
        {
            html.AppendLine("\t\t\t\t\t<td class=\"table-action\">");
            html.Append("\t\t\t\t\t\t");
            var link = GridView.ActionManager.GetLinkGrid(action, values);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, values);
                onRender.Invoke(this, args);
                if (args.ResultHtml != null)
                {
                    html.AppendLine(args.ResultHtml);
                    link = null;
                }
            }

            if (link != null)
                html.AppendLine(link.GetHtml());

            html.AppendLine("\t\t\t\t\t</td>");
        }

        if (listActionGroup.Count > 0)
        {
            html.AppendLine("\t\t\t\t\t<td class=\"table-action\">");
            html.AppendLine($"\t\t\t\t\t\t<div class=\"{BootstrapHelper.InputGroupBtn}\">");
            html.AppendLine(
                $"\t\t\t\t\t\t\t<{(BootstrapHelper.Version == 3 ? "button" : "a")} type=\"button\" class=\"btn-link dropdown-toggle\" {BootstrapHelper.DataToggle}=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">");
            html.Append('\t', 8);
            html.Append("<span class=\"caret\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.AppendFormat("title=\"{0}\">", Translate.Key("More Options"));
            html.AppendLine("</span>");
            html.AppendLine($"\t\t\t\t\t\t\t</{(BootstrapHelper.Version == 3 ? "button" : "a")}>");
            html.AppendLine("\t\t\t\t\t\t\t<ul class=\"dropdown-menu dropdown-menu-right\">");
            foreach (var action in listActionGroup)
            {
                var link = GridView.ActionManager.GetLinkGrid(action, values);
                var onRender = OnRenderAction;
                if (onRender != null)
                {
                    var args = new ActionEventArgs(action, link, values);
                    onRender.Invoke(this, args);
                }

                if (link is not { Visible: true }) continue;

                if (action.DividerLine)
                    html.AppendLine("\t\t\t\t\t\t\t\t<li role=\"separator\" class=\"divider\"></li>");

                html.AppendLine("\t\t\t\t\t\t\t\t<li class=\"dropdown-item\">");
                html.Append("\t\t\t\t\t\t\t\t\t");
                html.AppendLine(link.GetHtml());
                html.AppendLine("\t\t\t\t\t\t\t\t</li>");
            }

            html.AppendLine("\t\t\t\t\t\t\t</ul>");
            html.AppendLine("\t\t\t\t\t\t</div>");
            html.AppendLine("\t\t\t\t\t</td>");
        }


        return html.ToString();
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

            if (args.ResultHtml != null)
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