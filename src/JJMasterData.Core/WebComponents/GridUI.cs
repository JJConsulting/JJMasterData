using JJMasterData.Commons.Language;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
/// TODO: Breaking change suggestion: GridSettings is a more appropriate name for what the class represents
public class GridUI
{
    private const string TableTotalPerPage = "table_regperpage";
    private const string TableTotalPaginationButtons = "table_totalpagebuttons";
    private const string TableBorder = "table_border";
    private const string TableRowsStriped = "table_rowstriped";
    private const string TableRowHover = "table_rowhover";
    private const string TableIsHeaderFixed = "table_headerfixed";

    /// <summary>
    /// Settings cookie name.
    /// </summary>
    internal const string CookieName = "jjmasterdata_gridui";

    /// <remarks>
    /// (Default = 5)
    /// </remarks>
    public int TotalPerPage { get; set; }

    public int TotalPaginationButtons { get; set; }

    public bool ShowBorder { get; set; }

    public bool ShowRowStriped { get; set; }

    public bool ShowRowHover { get; set; }

    public bool IsResponsive { get; set; }

    public bool IsHeaderFixed { get; set; }

    public GridUI()
    {
        TotalPerPage = 5;
        TotalPaginationButtons = 5;
        ShowBorder = false;
        ShowRowStriped = true;
        ShowRowHover = true;
        IsResponsive = true;
    }


    internal static GridUI LoadFromForm(JJHttpContext currentContext)
    {
        var gridSettings = new GridUI();
        string tableRegPerPage = currentContext.Request[TableTotalPerPage];
        string tableTotalPageButtons = currentContext.Request[TableTotalPaginationButtons];
        string tableBorder = currentContext.Request[TableBorder];
        string tableRowsStriped = currentContext.Request[TableRowsStriped];
        string tableRowHover = currentContext.Request[TableRowHover];
        string tableIsHeaderFixed = currentContext.Request[TableIsHeaderFixed];

        if (int.TryParse(tableRegPerPage, out int totalPerPage))
            gridSettings.TotalPerPage = totalPerPage;

        if (int.TryParse(tableTotalPageButtons, out int totalPaggingButtons))
            gridSettings.TotalPaginationButtons = totalPaggingButtons;

        gridSettings.ShowBorder = "1".Equals(tableBorder);
        gridSettings.ShowRowStriped = "1".Equals(tableRowsStriped);
        gridSettings.ShowRowHover = "1".Equals(tableRowHover);
        gridSettings.IsHeaderFixed = "1".Equals(tableIsHeaderFixed);

        return gridSettings;
    }

    internal static bool HasFormValues(JJHttpContext currentContext) =>
        currentContext.Request[TableTotalPerPage] != null;

    internal HtmlElement GetHtmlElement(bool isPaginationEnabled)
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClass($"{(BootstrapHelper.Version == 3 ? "form-horizontal" : string.Empty)}")
            .WithAttribute("role", "form")
            .AppendHiddenInput(TableTotalPaginationButtons, TotalPaginationButtons.ToString())
            .AppendHiddenInput(TableIsHeaderFixed, IsHeaderFixed ? "1" : "0");

        if (isPaginationEnabled)
        {
            div.AppendElement(GetPaginationElement());
        }
        else
        {
            div.AppendHiddenInput(TableTotalPerPage, TotalPerPage.ToString());
        }

        div.AppendElement(GetShowBorderElement());
        div.AppendElement(GetShowRowsStripedElement());
        div.AppendElement(GetHighlightLineElement());

        return div;
    }

    private HtmlElement GetHighlightLineElement()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableRowHover);
                label.WithCssClass("col-sm-4");
                label.AppendText(Translate.Key("Highlight line on mouseover"));
            });
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.AppendElement(GetDataToggleElement(TableRowHover, ShowRowHover));
        });


        return div;
    }

    private HtmlElement GetShowRowsStripedElement()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableRowsStriped);
                label.WithCssClass("col-sm-4");
                label.AppendText(Translate.Key("Show rows striped"));
            });
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.AppendElement(GetDataToggleElement(TableRowsStriped, ShowRowStriped));
        });

        return div;
    }

    private HtmlElement GetShowBorderElement()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableBorder);
                label.WithCssClass("col-sm-4");
                label.AppendText(Translate.Key("Show table border"));
            });
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.AppendElement(GetDataToggleElement(TableBorder, ShowBorder));
        });


        return div;
    }

    private HtmlElement GetPaginationElement()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .AppendElement(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableTotalPerPage);
                label.WithCssClass("col-sm-4");
                label.AppendText(Translate.Key("Records per Page"));
            });
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-2");
            div.AppendElement(GetTotalPerPageSelectElement());
        });
        div.AppendElement(HtmlTag.Div, div => { div.WithCssClass("col-sm-6"); });

        return div;
    }

    private HtmlElement GetTotalPerPageSelectElement()
    {
        var select = new HtmlElement(HtmlTag.Select)
            .WithCssClass("form-control form-select")
            .WithNameAndId(TableTotalPerPage);

        for (int i = 1; i < 7; i++)
        {
            int page = i * 5;
            select.AppendElement(HtmlTag.Option, option =>
            {
                option.WithAttributeIf(TotalPerPage == page, "selected", "selected");
                option.WithValue(page.ToString());
                option.AppendText(page.ToString());
            });
        }

        return select;
    }

    private HtmlElement GetDataToggleElement(string name, bool isChecked)
    {
        var input = new HtmlElement(HtmlTag.Input)
            .WithAttribute("type", "checkbox")
            .WithValue("1")
            .WithCssClass("form-control")
            .WithNameAndId(name)
            .WithAttributeIf(isChecked, "checked", "checked")
            .WithAttribute("data-toggle", "toggle")
            .WithAttribute("data-on", Translate.Key("Yes"))
            .WithAttribute("data-off", Translate.Key("No"));

        return input;
    }
}