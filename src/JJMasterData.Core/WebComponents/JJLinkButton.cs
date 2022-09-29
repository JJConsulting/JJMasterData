using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.Html;
using System;

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

    internal override HtmlElement GetHtmlElement()
    {
        JJIcon icon = GetIcon();

        var html = new HtmlElement(HtmlTag.A);
        html.WithNameAndId(Name);
        html.WithCssClass(GetCssClassWithCompatibility());
        html.WithToolTip(Translate.Key(ToolTip));
        html.WithAttributes(Attributes);
        html.WithAttributeIf(Enabled && !string.IsNullOrEmpty(OnClientClick), "onclick", OnClientClick);
        html.WithCssClassIf(ShowAsButton, BootstrapHelper.DefaultButton);
        html.WithCssClassIf(!Enabled, "disabled");

        if (icon != null)
            html.AppendElement(icon.GetHtmlElement());

        if (!string.IsNullOrEmpty(Text))
            html.AppendElement(HtmlTag.Span, s =>
                {
                    s.AppendText("&nbsp " + Translate.Key(Text));
                });

        if (_spinner != null)
            html.AppendElement(Spinner.GetHtmlElement());

        if (IsSubmit)
        {
            html.Tag.TagName = HtmlTag.Button;
            html.WithAttribute("type", "submit");
            html.WithAttributeIf(ShowAsButton, "role", "button");
        }
        else
        {
            if (Enabled && !string.IsNullOrEmpty(UrlAction))
                html.WithAttribute("href", UrlAction);
            else
                html.WithAttribute("href", "javascript: void(0);");
        }

        return html;
    }

    private string GetCssClassWithCompatibility()
    {
        string cssClass = CssClass;
        if (cssClass == null)
            cssClass = string.Empty;

        if (BootstrapHelper.Version >= 4)
        {
            cssClass = cssClass.Replace("pull-left", BootstrapHelper.PullLeft);
            cssClass = cssClass.Replace("pull-right", BootstrapHelper.PullRight);
        }
        else
        {
            cssClass = cssClass.Replace("float-start", "pull-left");
            cssClass = cssClass.Replace("float-end", "pull-right");
        }

        if (IsSubmit)
        {
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
        }

        return cssClass;
    }

    private JJIcon GetIcon()
    {
        JJIcon icon = null;
        if (!string.IsNullOrEmpty(IconClass))
        {
            icon = new JJIcon(IconClass);
            if (!string.IsNullOrEmpty(IconClass) && IconClass.Contains("fa-") && IsGroup)
            {
                icon.CssClass = "fa-fw";
            }
        }

        return icon;
    }

}
