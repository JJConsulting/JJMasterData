using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Classe responsável por configurar a interface do usuário na JJGridView
/// </summary>
public class GridUI
{
    private const string TABLE_REGPERPAGE = "table_regperpage";
    private const string TABLE_TOTALPAGEBUTTONS = "table_totalpagebuttons";
    private const string TABLE_BORDER = "table_border";
    private const string TABLE_ROWSTRIPED = "table_rowstriped";
    private const string TABLE_ROWHOVER = "table_rowhover";
    private const string TABLE_HEADERFIXED = "table_headerfixed";

    /// <summary>
    /// Nome do cookie com as configurações padrões da grid
    /// </summary>
    internal const string CookieName = "jjmasterdata_gridui";

    /// <summary>
    /// Total de Registros por página 
    /// (Default = 5)
    /// </summary>
    /// <remarks>
    /// Se o TotalPerPage for zero a paginação não será exibida
    /// </remarks>
    public int TotalPerPage { get; set; }

    /// <summary>
    /// Total de botões na paginação 
    /// (Default = 5)
    /// </summary>
    public int TotalPaggingButton { get; set; }

    /// <summary>
    /// Exibi borda na grid 
    /// (Default = false)
    /// </summary>
    public bool ShowBorder { get; set; }

    /// <summary>
    /// Exibir colunas zebradas 
    /// (Default = true)
    /// </summary>
    public bool ShowRowStriped { get; set; }

    /// <summary>
    /// Alterar a cor da linha ao passar o mouse 
    /// (Default = true)
    /// </summary>
    public bool ShowRowHover { get; set; }

    /// <summary>
    /// Exibir barra de rolagem horizontal em aparelhos com a resolução inferior a 768px
    /// (Default = true)
    /// </summary>
    public bool IsResponsive { get; set; }


    /// <summary>
    /// Fixar o cabeçalho da grid ao realizar Scroll (Default = false)
    /// </summary>
    public bool HeaderFixed { get; set; }

    public GridUI()
    {
        TotalPerPage = 5;
        TotalPaggingButton = 5;
        ShowBorder = false;
        ShowRowStriped = true;
        ShowRowHover = true;
        IsResponsive = true;
    }


    internal static GridUI LoadFromForm(JJHttpContext currentContext)
    {
        var ret = new GridUI();
        string tableRegperpage = currentContext.Request[TABLE_REGPERPAGE];
        string tableTotalpagebuttons = currentContext.Request[TABLE_TOTALPAGEBUTTONS];
        string tableBorder = currentContext.Request[TABLE_BORDER];
        string tableRowstriped = currentContext.Request[TABLE_ROWSTRIPED];
        string tableRowhover = currentContext.Request[TABLE_ROWHOVER];
        string tableHeaderfixed = currentContext.Request[TABLE_HEADERFIXED];

        if (int.TryParse(tableRegperpage, out int regperpage))
            ret.TotalPerPage = regperpage;

        if (int.TryParse(tableTotalpagebuttons, out int totalpagebuttons))
            ret.TotalPaggingButton = totalpagebuttons;

        ret.ShowBorder = "1".Equals(tableBorder);
        ret.ShowRowStriped = "1".Equals(tableRowstriped);
        ret.ShowRowHover = "1".Equals(tableRowhover);
        ret.HeaderFixed = "1".Equals(tableHeaderfixed);

        return ret;
    }

    internal static bool HasFormValues(JJHttpContext currentContext) =>
        currentContext.Request[TABLE_REGPERPAGE] != null;
    
    internal string GetHtmlFormSetup(bool isPaggingEnable)
    {
        StringBuilder html = new();
        char TAB = '\t';

        html.Append(TAB, 5);
        html.AppendLine($"<div class=\"{(BootstrapHelper.Version == 3 ? "form-horizontal" : string.Empty)}\" role=\"form\"> ");

        html.Append(TAB, 6);
        html.Append("<input type=\"hidden\" ");
        html.AppendFormat("id=\"{0}\" ", TABLE_TOTALPAGEBUTTONS);
        html.AppendFormat("name=\"{0}\" ", TABLE_TOTALPAGEBUTTONS);
        html.AppendFormat("value=\"{0}\"", TotalPaggingButton);
        html.AppendLine("> ");

        html.Append(TAB, 6);
        html.Append("<input type=\"hidden\" ");
        html.AppendFormat("id=\"{0}\" ", TABLE_HEADERFIXED);
        html.AppendFormat("name=\"{0}\" ", TABLE_HEADERFIXED);
        html.AppendFormat("value=\"{0}\"", HeaderFixed ? "1" : "0");
        html.AppendLine("> ");

        if (isPaggingEnable)
        {
            html.Append(TAB, 6);
            html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} row\"> ");
            html.Append(TAB, 7);
            html.Append($"<label for=\"{TABLE_REGPERPAGE}\" class=\"col-sm-4\">");
            html.Append(Translate.Key("Records per Page"));
            html.AppendLine("</label>");
            html.Append(TAB, 7);
            html.AppendLine("<div class=\"col-sm-2\"> ");
            html.Append(TAB, 8);
            html.Append("<select class=\"form-control\" id=\"");
            html.Append(TABLE_REGPERPAGE);
            html.Append("\" name=\"");
            html.Append(TABLE_REGPERPAGE);
            html.AppendLine("\"> ");
            for (int i = 1; i < 7; i++)
            {
                int page = i * 5;
                html.Append(TAB, 9);
                html.Append("<option");
                html.Append(TotalPerPage == page ? " selected " : " ");
                html.Append("value =\"");
                html.Append(page);
                html.Append("\">");
                html.Append(page);
                html.AppendLine("</option> ");
            }
            html.Append(TAB, 8);
            html.AppendLine("</select> ");
            html.Append(TAB, 7);
            html.AppendLine("</div>");
            html.Append(TAB, 7);
            html.AppendLine("<div class=\"col-sm-6\"></div> ");
            html.Append(TAB, 6);
            html.AppendLine("</div> ");
        }
        else
        {
            html.Append(TAB, 6);
            html.Append("<input type=\"hidden\" id=\"");
            html.Append(TABLE_REGPERPAGE);
            html.Append("\" name=\"");
            html.Append(TABLE_REGPERPAGE);
            html.Append("\" value=\"");
            html.Append(TotalPerPage);
            html.AppendLine("\" /> ");
        }

        html.Append(TAB, 6);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} row\"> ");
        html.Append(TAB, 7);
        html.Append("<label for=\"");
        html.Append(TABLE_BORDER);
        html.Append("\" class=\"col-sm-4\">");
        html.Append(Translate.Key("Show table border"));
        html.AppendLine("</label> ");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-8\"> ");
        html.Append(TAB, 8);
        html.Append("<input type=\"checkbox\" ");
        html.Append("value =\"1\" ");
        html.Append("class=\"form-control\" id =\"");
        html.Append(TABLE_BORDER);
        html.Append("\" name=\"");
        html.Append(TABLE_BORDER);
        html.Append("\" ");
        html.Append(ShowBorder ? "checked=\"checked\" " : "");
        html.Append("data-toggle=\"toggle\" ");
        html.AppendFormat("data-on=\"{0}\" ", Translate.Key("Yes"));
        html.AppendFormat("data-off=\"{0}\"> ", Translate.Key("No"));
        html.AppendLine("");
        html.Append(TAB, 7);
        html.AppendLine("</div> ");
        html.Append(TAB, 6);
        html.AppendLine("</div> ");

        html.Append(TAB, 6);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} row\"> ");
        html.Append(TAB, 7);
        html.Append("<label for=\"");
        html.Append(TABLE_ROWSTRIPED);
        html.Append("\" class=\"col-sm-4\">");
        html.Append(Translate.Key("Show rows striped"));
        html.AppendLine("</label>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-8\"> ");
        html.Append(TAB, 8);
        html.Append("<input type=\"checkbox\" ");
        html.Append("value=\"1\" ");
        html.Append("class=\"form-control\" id =\"");
        html.Append(TABLE_ROWSTRIPED);
        html.Append("\" name=\"");
        html.Append(TABLE_ROWSTRIPED);
        html.Append("\" ");
        html.Append(ShowRowStriped ? "checked=\"checked\" " : "");
        html.Append("data-toggle=\"toggle\" ");
        html.AppendFormat("data-on=\"{0}\" ", Translate.Key("Yes"));
        html.AppendFormat("data-off=\"{0}\"> ", Translate.Key("No"));
        html.AppendLine("");
        html.Append(TAB, 7);
        html.AppendLine("</div> ");
        html.Append(TAB, 6);
        html.AppendLine("</div> ");

        html.Append(TAB, 6);
        html.AppendLine($"<div class=\"{BootstrapHelper.FormGroup} row\"> ");
        html.Append(TAB, 7);
        html.Append("<label for=\"");
        html.Append(TABLE_ROWHOVER);
        html.Append("\" class=\"col-sm-4\">");
        html.Append(Translate.Key("Highlight line on mouseover"));
        html.AppendLine("</label>");
        html.Append(TAB, 7);
        html.AppendLine("<div class=\"col-sm-8\"> ");
        html.Append(TAB, 8);
        html.Append("<input type=\"checkbox\" ");
        html.Append("value=\"1\" ");
        html.Append("class=\"form-control\" id=\"");
        html.Append(TABLE_ROWHOVER);
        html.Append("\" name=\"");
        html.Append(TABLE_ROWHOVER);
        html.Append("\" ");
        html.Append(ShowRowHover ? "checked=\"checked\" " : "");
        html.Append("data-toggle=\"toggle\" ");
        html.AppendFormat("data-on=\"{0}\" ", Translate.Key("Yes"));
        html.AppendFormat("data-off=\"{0}\"> ", Translate.Key("No"));
        html.AppendLine("");
        html.Append(TAB, 7);
        html.AppendLine("</div> ");
        html.Append(TAB, 6);
        html.AppendLine("</div> ");
        html.Append(TAB, 5);
        html.AppendLine("</div> ");

        return html.ToString();
    }
}
