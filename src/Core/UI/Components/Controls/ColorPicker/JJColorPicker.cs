using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components.ColorPicker;

public class JJColorPicker(IFormValues formValues) : ControlBase(formValues)
{
    
    public string Title { get; set; }

    public HtmlBuilder GetHtmlBuilder()
    {
        return new Input()
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