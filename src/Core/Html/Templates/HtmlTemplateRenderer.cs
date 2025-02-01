using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using JetBrains.Annotations;
using static JJMasterData.Core.Html.Templates.HtmlTemplateHelper;

namespace JJMasterData.Core.Html.Templates;

[PublicAPI]
public class HtmlTemplateRenderer(
    FluidParser fluidParser,
    HtmlTemplateHelper helper)
{
    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
            return new(error);

        var context = new TemplateContext(values);

        SetDefaultValues(context);

        return template.RenderAsync(context, HtmlEncoder.Default);
    }

    private void SetDefaultValues(TemplateContext context)
    {
        context.Options.Filters.AddFilter("localize", helper.GetLocalizeFilter());

        context.SetValue("isNullOrWhiteSpace", IsNullOrWhiteSpace);
        context.SetValue("isNullOrEmpty", IsNullOrEmpty);
        context.SetValue("substring", Substring);
        context.SetValue("formatDate", FormatDate);
        context.SetValue("trim", Trim);
        context.SetValue("trimStart", TrimStart);
        context.SetValue("trimEnd", TrimEnd);
        context.SetValue("dateAsText", helper.GetDatePhraseFunction());
        context.SetValue("urlPath", helper.GetUrlPathFunction());
        context.SetValue("appUrl", helper.GetAppUrlFunction());
        context.SetValue("localize", helper.GetLocalizeFunction());
    }
    
    public ValueTask<string> RenderTemplate(
        string templateString,
        TemplateContext context)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
            return new(error);

        SetDefaultValues(context);

        return template.RenderAsync(context, HtmlEncoder.Default);
    }
}