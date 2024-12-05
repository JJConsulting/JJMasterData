using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Localization;
using static JJMasterData.Core.Html.HtmlTemplateHelper;

namespace JJMasterData.Core.Html;

public class HtmlTemplateRenderer<TResource>(FluidParser fluidParser, IStringLocalizer<TResource> stringLocalizer)
{
    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
        {
            return new(error);
        }

        var context = new TemplateContext(values);
        
        context.Options.Filters.AddFilter("localize", GetLocalizeFilter(stringLocalizer));
        
        context.SetValue("isNullOrWhiteSpace", IsNullOrWhiteSpace);
        context.SetValue("isNullOrEmpty",IsNullOrEmpty);
        context.SetValue("substring", Substring);
        context.SetValue("formatDate", FormatDate);
        context.SetValue("localize", GetLocalizeFunction(stringLocalizer));

        return template.RenderAsync(context);
    }
}