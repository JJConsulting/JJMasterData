using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using static JJMasterData.Core.Html.HtmlTemplateFunctions;

namespace JJMasterData.Core.Html;

public class HtmlTemplateRenderer<TResource>(
    FluidParser fluidParser, 
    HtmlTemplateHelper<TResource> helper)
{
    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
        {
            return new(error);
        }

        var context = new TemplateContext(values);
        
        context.Options.Filters.AddFilter("localize", helper.GetLocalizeFilter());
        
        context.SetValue("isNullOrWhiteSpace", IsNullOrWhiteSpace);
        context.SetValue("isNullOrEmpty",IsNullOrEmpty);
        context.SetValue("substring", Substring);
        context.SetValue("formatDate", FormatDate);
        context.SetValue("dateAsText", helper.GetDatePhraseFunction());
        context.SetValue("urlPath", helper.GetUrlPathFunction());
        context.SetValue("localize", helper.GetLocalizeFunction());

        return template.RenderAsync(context);
    }
}