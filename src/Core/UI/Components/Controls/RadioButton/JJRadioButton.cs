using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

internal class JJRadioButton(IFormValues formValues) : ControlBase(formValues)
{
    public string Id { get; set; }
    public HtmlBuilder LabelHtml { get; } = new HtmlBuilder();
    public bool IsChecked { get; set; }
    public DataItemRadioLayout Layout { get; set; }
    
    protected override Task<ComponentResult> BuildResultAsync()
    {
        return Task.FromResult<ComponentResult>(new RenderedComponentResult(GetHtmlBuilder()));
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("form-check");
        div.WithCssClass(CssClass);
        div.WithCssClassIf(Layout is DataItemRadioLayout.Horizontal, "form-check-inline");
        div.AppendInput(input =>
        {
            input.WithName(Name);
            input.WithAttributeIf(!Enabled || ReadOnly, "disabled");
            input.WithCssClass("form-check-input");
            input.WithValue(Text);
            input.WithAttributes(Attributes);
            input.WithAttribute("type", "radio");
            input.WithAttributeIf(IsChecked, "checked");
        });
        div.AppendLabel(label =>
        {
            label.WithCssClass("form-check-label");
            label.WithAttribute("for", Id);
            label.Append(LabelHtml);
        });
        
        return div;
    }
}