using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using System.Linq;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents;

internal class GridTableBody
{
    private string Name => $"table_{GridView.Name}";
    private JJGridView GridView { get; }
    
    public EventHandler<ActionEventArgs> OnRenderAction { get; set; }
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

        var html = new HtmlElement();

        if (!isAjax)
        {
            html.AppendElement(HtmlTag.Tr, tr =>
            {
                tr.WithAttribute("id", $"row{index}");
                bool enableGridAction = !GridView.EnableEditMode && (defaultAction != null || GridView.EnableMultSelect);
                tr.WithCssClassIf(enableGridAction, "jjgrid-action");
            });
        }

        string onClickScript = GetOnClickScript(values, defaultAction);

        html.AppendElementIf(GridView.EnableMultSelect, GetMultiSelectRow(row, index, values, onClickScript));

        return html;
    }

    private HtmlElement GetMultiSelectRow(DataRow row, int index, Hashtable values, string onClickScript)
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
            onClickScript = $" onclick=\"$('#{checkBox.Name}').not(':disabled').prop('checked',!$('#{checkBox.Name}').is(':checked')).change();\"";
        }

        return td;
    }

    private string GetOnClickScript(Hashtable values, BasicAction defaultAction)
    {
        if (!GridView.EnableEditMode && defaultAction != null)
        {
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
                   return $" onclick =\"{linkDefaultAction.OnClientClick}\"";
                if (!string.IsNullOrEmpty(linkDefaultAction.UrlAction))
                   return $" onclick =\"window.location.href = '{linkDefaultAction.UrlAction}'\"";
            }
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