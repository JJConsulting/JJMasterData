using System;
using System.Collections;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Representa um Link 
/// </summary>
/// <example>
/// Exemplo de como utilizar JJLinkButton
/// <code lang="html">
/// <![CDATA[
///     <%@ Page Language="C#" AutoEventWireup="true"  %>
///     <!DOCTYPE html>
///     <html xmlns = "http://www.w3.org/1999/xhtml" >
///     <head runat="server">
///         <title></title>
///         <link rel = "Stylesheet" href="Content/bootstrap.css" />
///         <link rel = "stylesheet" href="Content/bootstrap-select.css" />
///         <link rel = "Stylesheet" href="Content/bootstrap-theme.css" />
///         <link rel = "Stylesheet" href="Content/jjmasterdata.css" />
/// 
///         <script type = "text/javascript" src="../Scripts/jquery-3.1.1.min.js"></script>
///         <script type = "text/javascript" src="../Scripts/bootstrap.min.js"></script>
///         <script type = "text/javascript" src="../Scripts/jjmasterdata.js"></script>
///     </head>
///     <body>
///         <form id = "form1" runat="server">
///         <%--Configuração do componente--%>
///         <%
///             var link = new JJMasterData.WebForm.JJLinkButton();
///             link.Text = "JJ Consulting";
///             link.IconClass = "glyphicon glyphicon-home";
///             link.UrlAction = "http://www.jjconsulting.com.br";
///         %>
/// 
///         <%--Rendereiza o componente--%>
///         <%=link.GetHtml() %>
/// 
///         </form>
///     </body>
///     </html>
/// ]]>
/// </code>
/// </example>
[Serializable]
public class JJLinkButton : JJBaseView, IAction
{

    private JJSpinner _spinner;

    public JJSpinner Spinner
    {
        get =>
            _spinner ??= new JJSpinner
            {
                Visible = false
            };
        set => _spinner = value;
    }


    /// <summary>
    /// Descrição do link
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Texto exibido quando o ponteiro do mouse passa sobre o controle
    /// </summary>
    public string ToolTip { get; set; }

    /// <summary>
    /// Executar essa ação como padrão.
    /// Ação será disparada ao clicar em qualquer lugar da linha.
    /// </summary>
    public bool IsDefaultOption { get; set; }

    /// <summary>
    /// Exibe a ação em um grupo de menu
    /// Default = false
    /// </summary>
    public bool IsGroup { get; set; }

    /// <summary>
    /// Faz um separador de menu antes dessa ação
    /// Default = false
    /// </summary>
    /// <remarks>
    /// Propriedade valida somente se o IsGroup for verdadeiro
    /// </remarks>
    public bool DividerLine { get; set; }

    /// <summary>
    /// Obtém ou define um valor que indica se o controle está habilitado.
    /// Default (true)
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <remarks>
    /// FontAwesome 2022 icon class.
    /// </remarks>
    public string IconClass { get; set; }

    /// <summary>
    /// Exibir com estilo de um botão (Default=false)
    /// </summary>
    public bool ShowAsButton { get; set; }

    /// <summary>
    /// Renderizar como botão ao invés de link
    /// </summary>
    public bool IsSubmit { get; set; }

    /// <summary>
    /// Ação JavaScript que será executada quando usuário clicar no controle
    /// </summary>
    public string OnClientClick { get; set; }

    /// <summary>
    /// URL que será usada para link quando um usuário clicar no controle
    /// </summary>
    public string UrlAction { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe JJButton
    /// </summary>
    public JJLinkButton()
    {
        Enabled = true;
    }


    protected override string RenderHtml()
    {
        ApplyCompatibility();

        return IsSubmit ? RenderButton() : RenderLink();
    }

    private void ApplyCompatibility()
    {
        if (BootstrapHelper.Version < 4) return;
        
        CssClass = CssClass?.Replace("pull-left", BootstrapHelper.PullLeft);
        CssClass = CssClass?.Replace("pull-right", BootstrapHelper.PullRight);
    }


    private string RenderLink()
    {
        var html = new StringBuilder();

        html.Append("<a href=\"");
        if (!string.IsNullOrEmpty(UrlAction) && Enabled)
        {
            html.Append(UrlAction);
        }
        else
        {
            html.Append("javascript: void(0);");
        }
        html.Append('"');


        if (!string.IsNullOrEmpty(Name))
            html.AppendFormat(" id=\"{0}\"", Name);

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key(ToolTip));
            html.Append("\"");
        }

        string cssClass = CssClass;
        if (cssClass == null)
            cssClass = "";

        if (ShowAsButton)
        {
            html.Append(" role=\"button\"");

            cssClass += BootstrapHelper.DefaultButton;
        }

        if (!Enabled)
        {
            cssClass += " disabled";
        }

        if (!string.IsNullOrEmpty(cssClass))
        {
            html.Append(" class=\"");
            html.Append(cssClass);
            html.Append('"');
        }

        if (!string.IsNullOrEmpty(OnClientClick) && Enabled)
        {
            html.Append(" onclick=\"");
            html.Append(OnClientClick);
            html.Append('"');
        }

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(' ');
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append('"');
            }
        }

        html.Append(">");

        if (!string.IsNullOrEmpty(IconClass))
        {
            var icon = new JJIcon(IconClass);
            if (!string.IsNullOrEmpty(IconClass) &&
                IconClass.Contains("fa-") &&
                IsGroup)
            {
                icon.CssClass = "fa-fw";
            }


            html.Append(icon.GetHtml());
        }

        if (!string.IsNullOrEmpty(Text))
        {
            html.Append("<span>");
            html.Append("&nbsp " + Translate.Key(Text));
            html.Append("</span>");
        }

        if (_spinner != null)
            html.Append(Spinner.GetHtml());

        html.Append("</a>");

        return html.ToString();
    }


    private string RenderButton()
    {
        StringBuilder html = new StringBuilder();

        html.Append("<button type=\"submit\"");


        if (!string.IsNullOrEmpty(Name))
            html.AppendFormat(" id=\"{0}\"", Name);

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($" {BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key(ToolTip));
            html.Append("\"");
        }

        string cssClass = CssClass;
        if (cssClass == null)
            cssClass = string.Empty;

        if (!cssClass.Contains("btn ") &&
            !cssClass.Contains(" btn") &&
            !cssClass.Equals("btn"))
        {
            cssClass += " btn";
        }

        if (!cssClass.Contains("btn-default") ||
            !cssClass.Contains("btn-primary"))
        {
            cssClass += BootstrapHelper.DefaultButton;
        }

        if (!Enabled)
        {
            cssClass += " disabled";
        }
        
        if (!string.IsNullOrEmpty(cssClass))
        {
            html.Append(" class=\"");
            html.Append(cssClass);
            html.Append("\"");
        }

        if (!string.IsNullOrEmpty(OnClientClick) && Enabled)
        {
            html.Append(" onclick=\"");
            html.Append(OnClientClick);
            html.Append("\"");
        }

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(" ");
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
        }

        html.Append(">");

        if (!string.IsNullOrEmpty(IconClass))
        {
            var icon = new JJIcon(IconClass);
            if (!string.IsNullOrEmpty(IconClass) &&
                IconClass.Contains("fa-") &&
                IsGroup)
            {
                icon.CssClass = "fa-fw";
            }


            html.Append(icon.GetHtml());
        }

        if (!string.IsNullOrEmpty(Text))
        {
            html.Append("<span>&nbsp;");
            html.Append(Translate.Key(Text));
            html.Append("</span>");
        }

        html.Append("</button>");

        return html.ToString();
    }

    /// <summary>
    /// Retorna uma nova instancia do mesmo objeto
    /// </summary>
    public JJLinkButton GetClone()
    {
        var c = new JJLinkButton();
        c.Attributes = Attributes;
        c.Enabled = Enabled;
        c.ToolTip = ToolTip;
        c.Text = Text;
        c.CssClass = CssClass;
        c.IconClass = IconClass;
        c.ShowAsButton = ShowAsButton;
        c.OnClientClick = OnClientClick;
        c.UrlAction = UrlAction;
        c.Visible = Visible;
        c.IsGroup = IsGroup;
        c.DividerLine = DividerLine;
        return c;
    }

}
