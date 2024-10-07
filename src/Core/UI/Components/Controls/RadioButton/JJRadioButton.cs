using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

internal sealed class JJRadioButton(IFormValues formValues) : ControlBase(formValues)
{
    public string Id { get; set; }
    public HtmlBuilder LabelHtml { get; } = new();
    public bool IsChecked { get; set; }
    public DataItemRadioLayout Layout { get; set; }
    
    protected override ValueTask<ComponentResult> BuildResultAsync()
    {
        return new ValueTask<ComponentResult>(new RenderedComponentResult(GetHtmlBuilder()));
    }
    

    public HtmlBuilder GetHtmlBuilder()
    {
        var showAsButton = Layout is DataItemRadioLayout.Buttons;
        
        var parent = !showAsButton ? new HtmlBuilder(HtmlTag.Div) : new();
        parent.WithCssClass("form-check");
        parent.WithCssClass(CssClass);
    
        parent.WithCssClassIf(Layout is DataItemRadioLayout.Horizontal, "form-check-inline");
    
        parent.AppendInput(input =>
        {
            input.WithName(Name);
            input.WithId(Id);
            input.WithAttributeIf(!Enabled || ReadOnly, "disabled");
            input.WithCssClass(showAsButton ? "btn-check" : "form-check-input");
            input.WithValue(Text);
            input.WithAttributes(Attributes);
            input.WithAttribute("type", "radio");
            input.WithAttributeIf(IsChecked, "checked");
        });

        parent.AppendLabel(label =>
        {
            label.WithCssClass(showAsButton ? "btn btn-outline-primary" : "form-check-label");
            label.WithAttribute("for", Id);
            label.Append(LabelHtml);
        });

        return parent;
    }
}