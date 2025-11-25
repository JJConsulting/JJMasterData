using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridSortingConfig(JJGridView gridView)
{
    private readonly FormElement _formElement = gridView.FormElement;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer = gridView.StringLocalizer;
    private readonly ExpressionsService _expressionsService = gridView.ExpressionsService;
    private readonly IComponentFactory _componentFactory = gridView.ComponentFactory;

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var dialog = _componentFactory.Html.ModalDialog.Create();
        dialog.Name = $"{gridView.Name}-sort-modal";
        dialog.Title = _stringLocalizer["Sort Fields"];
        dialog.Size = ModalSize.Small;

        var btnSort = _componentFactory.Html.LinkButton.Create();
        btnSort.Name = $"btnsort_{gridView.Name}";
        btnSort.IconClass = IconType.Check.GetCssClass();
        btnSort.ShowAsButton = true;
        btnSort.Text = _stringLocalizer["Sort"];
        btnSort.OnClientClick = gridView.Scripts.GetSortMultItemsScript();
        dialog.Buttons.Add(btnSort);

        var btnCancel = _componentFactory.Html.LinkButton.Create();
        btnCancel.Text = _stringLocalizer["Cancel"];
        btnCancel.IconClass = IconType.Times.GetCssClass();
        btnCancel.ShowAsButton = true;
        btnCancel.OnClientClick = BootstrapHelper.GetCloseModalScript($"{gridView.Name}-sort-modal");
        dialog.Buttons.Add(btnCancel);

        var htmlContent = new HtmlBuilder(HtmlTag.Div)
            .AppendComponent(new JJAlert
            {
                Title = _stringLocalizer["Drag and drop to change order."],
                Icon = IconType.SolidCircleInfo,
                Color = BootstrapColor.Info,
                ShowIcon = true,
                ShowCloseButton = false
            });

        var body = await GetHtmlBody();
        
        htmlContent.Append(HtmlTag.Table, table =>
        {
            table.WithCssClass("table table-hover");
            table.Append(GetHtmlHeader());
            table.Append(body);
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
                th.WithStyle("width:50px");
                th.AppendText("#");
            });
            tr.Append(HtmlTag.Th, th => th.AppendText(gridView.StringLocalizer["Column"]));
            tr.Append(HtmlTag.Th, th =>
            {
                th.WithStyle("width:220px");
                th.AppendText(gridView.StringLocalizer["Order"]);
            });
        });

        return thead;
    }

    private async Task<HtmlBuilder> GetHtmlBody()
    {
        var tbody = new HtmlBuilder(HtmlTag.Tbody);
        tbody.WithAttribute("id", $"sortable-{gridView.Name}");
        tbody.WithCssClass("ui-sortable jjsortable");

        var sortList = GetSortList();
        var fieldsList = sortList.ConvertAll(sort => _formElement.Fields[sort.FieldName]);

        foreach (var item in _formElement.Fields)
        {
            var field = fieldsList.Find(x => x.Name.Equals(item.Name));
            if (field == null)
                fieldsList.Add(item);
        }

        var formStateData = await gridView.GetFormStateDataAsync();

        foreach (var field in fieldsList)
        {
            if (field.DataBehavior is FieldBehavior.Real &&
                _expressionsService.GetBoolValue(field.VisibleExpression, formStateData))
            {
                var comboBox = _componentFactory.Controls.ComboBox.Create();
                comboBox.Name = $"{field.Name}_order";
                comboBox.SelectedValue = "N";
                comboBox.DataItem.ShowIcon = true;
                comboBox.DataItem.Items =
                [
                    new("A", _stringLocalizer["Ascendant"], IconType.SortAmountAsc, null),
                    new("D", _stringLocalizer["Descendant"], IconType.SortAmountDesc, null),
                    new("N", _stringLocalizer["No Order"], IconType.Genderless, null)
                ];

                var sort = sortList.Find(x => x.FieldName.Equals(field.Name));
                if (sort != null)
                {
                    comboBox.SelectedValue = sort.IsAsc ? "A" : "D";
                }

                var comboHtml = await comboBox.GetHtmlBuilderAsync();
                
                tbody.Append(HtmlTag.Tr, tr =>
                {
                    tr.WithAttribute("id", field.Name);
                    tr.WithCssClass("ui-sortable-handle");
                    tr.Append(HtmlTag.Td, td => td.AppendComponent(new JJIcon("fa fa-arrows")));
                    tr.Append(HtmlTag.Td, td => td.AppendText(_stringLocalizer[field.LabelOrName]));
                    tr.Append(HtmlTag.Td, td => td.Append(comboHtml));
                });
            }
        }

        return tbody;
    }

    private List<SortItem> GetSortList()
    {
        var sortList = new List<SortItem>();
        var currentOrder = gridView.CurrentOrder.ToQueryParameter();
        if (string.IsNullOrEmpty(currentOrder))
            return sortList;

        var orders = currentOrder.Split(',');
        foreach (var order in orders)
        {
            var parValue = order.Split(' ');
            var isDesc = parValue.Length > 1 &&
                         parValue[1].Trim().Equals("DESC", System.StringComparison.OrdinalIgnoreCase);
            var sort = new SortItem(
                parValue[0].Trim(),
                !isDesc
            );
            sortList.Add(sort);
        }

        return sortList;
    }

    private sealed class SortItem(string fieldName, bool isAsc)
    {
        public string FieldName { get; } = fieldName;
        public bool IsAsc { get; } = isAsc;
    }
}