#nullable enable

using System.Globalization;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;


namespace JJMasterData.Core.UI.Components;

public class JJCodeEditor(IFormValues formValues) : ControlBase(formValues)
{
    public string Language { get; set; } = "html";
    public int Height { get; set; } = 300;
    
    protected internal override ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = GetHtmlBuilder();
        
        return html.AsValueTask();
    }

    public HtmlBuilder GetHtmlBuilder()
    {
        var editorId = (Name + "-editor").Replace(".", "_");

        var style = new HtmlBuilder(HtmlTag.Style)
            .AppendText($"#{editorId} {{ height: {(Height * 1.3).ToString(CultureInfo.InvariantCulture)}px; }}")
            .AppendText($"@media (max-width: 1200px) {{ #{editorId} {{ height: {Height}px; }} }}")
            .AppendText($"@media (max-width: 768px) {{ #{editorId} {{ height: {(Height * 0.7).ToString(CultureInfo.InvariantCulture)}px; }} }}")
            .AppendText($"@media (max-width: 480px) {{ #{editorId} {{ height: {(Height * 0.4).ToString(CultureInfo.InvariantCulture)}px; }} }}");

        var wrapper = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("w-100 h-100")
            .WithCssClass("jj-code-editor")
            .WithAttribute("data-editor-id",editorId)
            .WithAttribute("data-editor-name",Name)
            .WithAttribute("data-readonly", (!Enabled).ToString().ToLowerInvariant())
            .WithAttribute("data-language", Language)
            .AppendTextArea(textArea =>
            {
                textArea.WithAttribute("hidden", "hidden")
                    .WithName(Name)
                    .WithId(Name)
                    .AppendText(Text);
            })
            .AppendDiv(div => div.WithId(editorId));

        var html = new HtmlBuilder()
            .Append(style)
            .Append(wrapper);
        return html;
    }
}
