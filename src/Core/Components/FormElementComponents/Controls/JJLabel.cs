using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Representa um Label padrão
/// </summary>
public class JJLabel : JJBaseView
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
    public JJLabel(FormElementField f)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        LabelFor = f.Name;
        Text = string.IsNullOrEmpty(f.Label) ? f.Name : f.Label;
        ToolTip = f.HelpDescription;
        IsRequired = f.IsRequired;
    }

    internal override HtmlBuilder RenderHtml()
    {
        var element = new HtmlBuilder(HtmlTag.Label)
            .WithNameAndId(Name)
            .WithAttributes(Attributes)
            .WithCssClass(BootstrapHelper.Label)
            .WithCssClass(CssClass)
            .AppendText(Translate.Key(Text))
            .AppendElementIf(IsRequired, HtmlTag.Span, s =>
            {
                s.WithCssClass("required-symbol");
                s.AppendText("*");
                s.WithToolTip(Translate.Key("Required"));
            })
            .AppendElementIf(!string.IsNullOrEmpty(ToolTip), HtmlTag.Span, s =>
            {
                s.WithCssClass("fa fa-question-circle help-description");
                s.WithToolTip(Translate.Key(ToolTip));
            });
          
        return element;
    }
}
