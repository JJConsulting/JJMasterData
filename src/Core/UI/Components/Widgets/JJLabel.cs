using System;
using JetBrains.Annotations;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.UI.Components;
namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Representa um Label padrão
/// </summary>
public class JJLabel : HtmlComponent
{
    /// <summary>
    /// Texto exibido quando o ponteiro do mouse passa sobre o controle
    /// </summary>
    /// 
    public string ToolTip { get; set; }

    /// <summary>
    /// Nome do controle referente ao label
    /// </summary>
    public string LabelFor
    {
        get => GetAttr("for");
        set => SetAttr("for", value);
    }

    /// <summary>
    /// Descrição do label
    /// </summary>
    public string Text { get; set; }
    public string RequiredText { get; set; }
    public bool IsRequired { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe JJLabel
    /// </summary>
    public JJLabel()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância da classe JJLabel a partir de um FormElementField
    /// </summary>
    public JJLabel(FormElementField field)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

        LabelFor = field.Name;
        Text = field.LabelOrName;
        ToolTip = field.HelpDescription;
        IsRequired = field.IsRequired;
    }

    internal override HtmlBuilder BuildHtml()
    {
        var element = new HtmlBuilder(HtmlTag.Label)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(BootstrapHelper.Label)
            .WithCssClass(CssClass)
            .AppendText(Text)
            .AppendIf(IsRequired, HtmlTag.Span, s =>
            {
                s.WithCssClass("required-symbol");
                s.AppendText("*");
                s.WithToolTip(RequiredText ?? "Required");
            })
            .AppendIf(!string.IsNullOrEmpty(ToolTip), HtmlTag.Span, s =>
            {
                s.WithCssClass("fa fa-question-circle help-description");
                s.WithToolTip(ToolTip);
            });
          
        return element;
    }
}
