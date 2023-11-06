using System.ComponentModel;
using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components.ColorPicker;

public class JJColorPicker : ControlBase
{
    
    public string Title { get; set; }
    
    public JJColorPicker(IFormValues formValues) : base(formValues)
    {
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        return new HtmlBuilder(HtmlTag.Input)
            .WithNameAndId(Name)
            .WithValue(Text)
            .WithAttribute("title", Title)
            .WithAttribute("type","color")
            .WithAttributeIf(ReadOnly, "readonly", "readonly")
            .WithAttributeIf(!Enabled, "disabled", "disabled")
            .WithCssClass("form-control form-control-color");
    }
    
    protected override Task<ComponentResult> BuildResultAsync()
    {
        return Task.FromResult<ComponentResult>(new RenderedComponentResult(GetHtmlBuilder()));
    }
}