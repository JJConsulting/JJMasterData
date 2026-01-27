using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;


namespace JJMasterData.Core.UI.Components.ColorPicker;

public class JJColorPicker(IFormValues formValues) : ControlBase(formValues)
{
    public string Title { get; set; }

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

    protected internal override ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = GetHtmlBuilder();

        return html.AsValueTask();
    }
}