using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class GridConfigSorting
{
    public string CurrentOrder { get; set; }
    public FormElement FormElement { get; set; }

    public string Name { get; set; }

    public GridConfigSorting(JJGridView grid)
    {
        if (grid == null)
            throw new ArgumentNullException(nameof(grid));

        CurrentOrder = grid.CurrentOrder;
        FormElement = grid.FormElement;
        Name = grid.Name;
    }

    public string GetHtml()
    {
        var dialog = new JJModalDialog
        {
            Name = "sort_modal_" + Name,
            Title = "Sort Fields",
            Size = MessageSize.Default
        };

        var btnSort = new JJLinkButton
        {
            Name = "btnsort_" + Name,
            IconClass = "fa fa-check",
            ShowAsButton = true,
            Text = "Sort",
            OnClientClick = $"jjview.doMultSorting('{Name}');"
        };
        dialog.Buttons.Add(btnSort);

        var btnCancel = new JJLinkButton
        {
            Text = "Cancel",
            IconClass = "fa fa-trash",
            ShowAsButton = true,
            OnClientClick = $"$('#sort_modal_{Name}').modal('hide');"
        };
        dialog.Buttons.Add(btnCancel);

        var cab = new StringBuilder();
        cab.Append('\t', 5);
        cab.Append("<div>");
        cab.Append("<span class=\"text-info fa fa-triangle-exclamation\"></span>");
        cab.Append("&nbsp;");
        cab.Append(Translate.Key("Drag and drop to change order."));
        cab.AppendLine("</div>");
        cab.Append('\t', 5);
        cab.AppendLine("<table class=\"table table-hover\">");
        cab.Append('\t', 6);
        cab.AppendLine("<thead>");
        cab.Append('\t', 7);
        cab.AppendLine("<tr>");
        cab.Append('\t', 8);
        cab.AppendLine("<th style=\"width:50px;\">#</th>");
        cab.Append('\t', 8);
        cab.Append("<th>");
        cab.Append(Translate.Key("Column"));
        cab.AppendLine("</th>");
        cab.Append('\t', 8);
        cab.Append("<th style=\"width:222px;\">");
        cab.Append(Translate.Key("Order"));
        cab.AppendLine("</th>");
        cab.Append('\t', 7);
        cab.AppendLine("</tr>");
        cab.Append('\t', 6);
        cab.AppendLine("</thead>");
        cab.Append('\t', 6);
        cab.AppendLine($"<tbody id=\"sortable_{Name}\" class=\"ui-sortable jjsortable\">");

        var cbo = new JJComboBox();
        cbo.DataItem.ShowImageLegend = true;
        cbo.DataItem.Itens.Add(new DataItemValue("A", "Ascendant", IconType.SortAmountAsc, null));
        cbo.DataItem.Itens.Add(new DataItemValue("D", "Descendant", IconType.SortAmountDesc, null));
        cbo.DataItem.Itens.Add(new DataItemValue("N", "Not order", IconType.Genderless, null));

        var listsort = GetListSort();
        var listFields = new List<FormElementField>();

        foreach (var sort in listsort)
        {
            var f = FormElement.Fields[sort.FieldName];
            listFields.Add(f);
        }

        foreach (var item in FormElement.Fields)
        {
            var f = listFields.Find(x => x.Name.Equals(item.Name));
            if (f == null)
                listFields.Add(item);
        }

        foreach (var item in listFields)
        {
            if (item.VisibleExpression.Equals("val:0"))
                continue;

            cbo.Name = item.Name + "_order";
            cbo.SelectedValue = "N";

            var sort = listsort.Find(x => x.FieldName.Equals(item.Name));
            if (sort != null)
            {
                if (sort.IsAsc)
                    cbo.SelectedValue = "A";
                else
                    cbo.SelectedValue = "D";
            }

            cab.Append('\t', 7);
            cab.Append("<tr id='");
            cab.Append(item.Name);
            cab.AppendLine("' class=\"ui-sortable-handle\">");
            cab.Append('\t', 8);
            cab.Append("<td>");
            cab.Append("<span class=\"fa fa-arrows\"></span>");
            cab.AppendLine("</td>");
            cab.Append('\t', 8);
            cab.Append("<td>");
            cab.Append(Translate.Key(item.Label));
            cab.AppendLine("</td>");
            cab.Append('\t', 8);
            cab.AppendLine("<td>");
            cab.Append('\t', 9);
            cab.AppendLine(cbo.GetHtml());
            cab.Append('\t', 8);
            cab.AppendLine("</td>");
            cab.Append('\t', 7);
            cab.AppendLine("</tr>");
        }

        cab.Append('\t', 6);
        cab.AppendLine("</tbody>");
        cab.Append('\t', 5);
        cab.AppendLine("</table>");

        dialog.HtmlContent = cab.ToString();

        return dialog.GetHtml();
    }

    private List<SortItem> GetListSort()
    {
        var listsort = new List<SortItem>();
        if (string.IsNullOrEmpty(CurrentOrder))
            return listsort;

        var orders = CurrentOrder.Split(',');
        foreach (string order in orders)
        {
            var parValue = order.Split(' ');
            var sort = new SortItem
            {
                FieldName = parValue[0].Trim(),
                IsAsc = true
            };
            if (parValue.Length > 1 && parValue[1].Trim().ToUpper().Equals("DESC"))
            {
                sort.IsAsc = false;
            }
            listsort.Add(sort);
        }

        return listsort;
    }

    private class SortItem
    {
        public string FieldName { get; set; }
        public bool IsAsc { get; set; }
    }

}
