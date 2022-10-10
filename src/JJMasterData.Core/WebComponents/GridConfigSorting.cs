using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

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

    public HtmlElement GetHtmlElement()
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
            IconClass = IconHelper.GetClassName(IconType.Check),
            ShowAsButton = true,
            Text = "Sort",
            OnClientClick = $"jjview.doMultSorting('{Name}');"
        };
        dialog.Buttons.Add(btnSort);

        var btnCancel = new JJLinkButton
        {
            Text = "Cancel",
            IconClass = IconHelper.GetClassName(IconType.Times),
            ShowAsButton = true,
            OnClientClick = $"$('#sort_modal_{Name}').modal('hide');"
        };

        dialog.Buttons.Add(btnCancel);

        var htmlContent = new HtmlElement(HtmlTag.Div)
            .AppendElement(HtmlTag.Div, div =>
            {
                div.AppendElement(new JJIcon("text-info fa fa-triangle-exclamation"));
                div.AppendText("&nbsp;");
                div.AppendText(Translate.Key("Drag and drop to change order."));
            })
            .AppendElement(HtmlTag.Table, table =>
            {
                table.WithCssClass("table table-hover");
                table.AppendElement(GetHtmlHeader());
                table.AppendElement(GetHtmlBody());
            });

        dialog.HtmlElementContent = htmlContent;

        return dialog.GetHtmlElement();
    }

    private static HtmlElement GetHtmlHeader()
    {
        var thead = new HtmlElement(HtmlTag.Thead);
        thead.AppendElement(HtmlTag.Tr, tr =>
        {
            tr.AppendElement(HtmlTag.Th, th =>
            {
                th.WithAttribute("style", "width:50px");
                th.AppendText("#");
            });
            tr.AppendElement(HtmlTag.Th, th =>
            {
                th.AppendText(Translate.Key("Column"));
            });
            tr.AppendElement(HtmlTag.Th, th =>
            {
                th.WithAttribute("style", "width:220px");
                th.AppendText(Translate.Key("Order"));
            });
        });

        return thead;
    }

    private HtmlElement GetHtmlBody()
    {
        var tbody = new HtmlElement(HtmlTag.Tbody);
        tbody.WithAttribute("id", $"sortable_{Name}");
        tbody.WithCssClass("ui-sortable jjsortable");

        var comboBox = new JJComboBox
        {
            DataItem =
            {
                ShowImageLegend = true,
                Items = new List<DataItemValue>
                {
                    new("A", Translate.Key("Ascendant"), IconType.SortAmountAsc, null),
                    new("D", Translate.Key("Descendant"), IconType.SortAmountDesc, null),
                    new("N", Translate.Key("No Order"), IconType.Genderless, null)
                }
            },
        };

        var sortList = GetSortList();
        var fieldsList = sortList.Select(sort => FormElement.Fields[sort.FieldName]).ToList();

        foreach (var item in FormElement.Fields)
        {
            var f = fieldsList.Find(x => x.Name.Equals(item.Name));
            if (f == null)
                fieldsList.Add(item);
        }

        foreach (var item in fieldsList.Where(item => !item.VisibleExpression.Equals("val:0")))
        {
            comboBox.Name = item.Name + "_order";
            comboBox.SelectedValue = "N";

            var sort = sortList.Find(x => x.FieldName.Equals(item.Name));
            if (sort != null)
            {
                comboBox.SelectedValue = sort.IsAsc ? "A" : "D";
            }

            tbody.AppendElement(HtmlTag.Tr, tr =>
            {
                tr.WithAttribute("id", item.Name);
                tr.WithCssClass("ui-sortable-handle");
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendElement(new JJIcon("fa fa-arrows"));
                });
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendText(Translate.Key(item.Label ?? item.Name));
                });
                tr.AppendElement(HtmlTag.Td, td =>
                {
                    td.AppendElement(comboBox);
                });
            });
        }

        return tbody;
    }

    private List<SortItem> GetSortList()
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
