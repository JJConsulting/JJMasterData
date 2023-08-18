using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public class GridSortingConfig
{
    public string CurrentOrder { get; set; }

    public FormElement FormElement { get; set; }
    public IControlFactory<JJComboBox> ComboBoxFactory { get; set; }
    public string Name { get; set; }
    
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    
    public GridSortingConfig(JJGridView grid)
    {
        if (grid == null)
            throw new ArgumentNullException(nameof(grid));

        CurrentOrder = grid.CurrentOrder;
        ComboBoxFactory = grid.ComponentFactory.Controls.GetFactory<ComboBoxFactory>();
        StringLocalizer = grid.StringLocalizer;
        FormElement = grid.FormElement;
        Name = grid.Name;
    }

    public HtmlBuilder GetHtmlElement()
    {
        var dialog = new JJModalDialog
        {
            Name = "sort-modal-" + Name,
            Title = "Sort Fields",
            Size = MessageSize.Default
        };

        var btnSort = new JJLinkButton
        {
            Name = "btnsort_" + Name,
            IconClass = IconType.Check.GetCssClass(),
            ShowAsButton = true,
            Text = "Sort",
            OnClientClick = $"JJView.sortItems('{Name}');"
        };
        dialog.Buttons.Add(btnSort);

        var btnCancel = new JJLinkButton
        {
            Text = "Cancel",
            IconClass = IconType.Times.GetCssClass(),
            ShowAsButton = true,
            OnClientClick = $"$('#sort-modal-{Name}').modal('hide');"
        };

        dialog.Buttons.Add(btnCancel);

        var htmlContent = new HtmlBuilder(HtmlTag.Div)
            .Append(HtmlTag.Div, div =>
            {
                div.AppendComponent(new JJIcon("text-info fa fa-triangle-exclamation"));
                div.AppendText("&nbsp;");
                div.AppendText(StringLocalizer["Drag and drop to change order."]);
            })
            .Append(HtmlTag.Table, table =>
            {
                table.WithCssClass("table table-hover");
                table.Append(GetHtmlHeader());
                table.Append(GetHtmlBody());
            });

        dialog.HtmlBuilderContent = htmlContent;

        return dialog.BuildHtml();
    }

    private HtmlBuilder GetHtmlHeader()
    {
        var thead = new HtmlBuilder(HtmlTag.Thead);
        thead.Append(HtmlTag.Tr, tr =>
        {
            tr.Append(HtmlTag.Th, th =>
            {
                th.WithAttribute("style", "width:50px");
                th.AppendText("#");
            });
            tr.Append(HtmlTag.Th, th =>
            {
                th.AppendText(StringLocalizer["Column"]);
            });
            tr.Append(HtmlTag.Th, th =>
            {
                th.WithAttribute("style", "width:220px");
                th.AppendText(StringLocalizer["Order"]);
            });
        });

        return thead;
    }

    private HtmlBuilder GetHtmlBody()
    {
        var tbody = new HtmlBuilder(HtmlTag.Tbody);
        tbody.WithAttribute("id", $"sortable-{Name}");
        tbody.WithCssClass("ui-sortable jjsortable");

        var comboBox = ComboBoxFactory.Create();
        comboBox.DataItem.ShowImageLegend = true;
        comboBox.DataItem.Items = new List<DataItemValue>
        {
            new("A", StringLocalizer["Ascendant"], IconType.SortAmountAsc, null),
            new("D", StringLocalizer["Descendant"], IconType.SortAmountDesc, null),
            new("N", StringLocalizer["No Order"], IconType.Genderless, null)
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

            tbody.Append(HtmlTag.Tr, tr =>
            {
                tr.WithAttribute("id", item.Name);
                tr.WithCssClass("ui-sortable-handle");
                tr.Append(HtmlTag.Td, td =>
                {
                    td.AppendComponent(new JJIcon("fa fa-arrows"));
                });
                tr.Append(HtmlTag.Td, td =>
                {
                    td.AppendText(StringLocalizer[item.LabelOrName]);
                });
                tr.Append(HtmlTag.Td, td =>
                {
                    td.AppendComponent(comboBox);
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
