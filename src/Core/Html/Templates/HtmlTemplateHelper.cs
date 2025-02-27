using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using JetBrains.Annotations;
using static JJMasterData.Core.Html.Templates.HtmlTemplateFunctions;

namespace JJMasterData.Core.Html.Templates;

[PublicAPI]
public class HtmlTemplateHelper(
    FluidParser fluidParser,
    HtmlTemplateFunctions functions
    )
{
    public IFluidTemplate ParseTemplate(string template)
    {
        return fluidParser.Parse(template);
    }
    
    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
            return new(error);

        var context = new TemplateContext(values, allowModelMembers:true, StringComparer.InvariantCultureIgnoreCase);

        SetDefaultValues(context);

        return template.RenderAsync(context, HtmlEncoder.Default);
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
    
    public ValueTask<string> RenderTemplate(IFluidTemplate template, TemplateContext context)
    {
        SetDefaultValues(context);
        
        return template.RenderAsync(context, HtmlEncoder.Default);
    }
    
    private void SetDefaultValues(TemplateContext context)
    {
        context.SetValue("isNullOrWhiteSpace", IsNullOrWhiteSpace);
        context.SetValue("isNullOrEmpty", IsNullOrEmpty);
        context.SetValue("substring", Substring);
        context.SetValue("capitalize", Capitalize);
        context.SetValue("formatDate", FormatDate);
        context.SetValue("trim", Trim);
        context.SetValue("trimStart", TrimStart);
        context.SetValue("trimEnd", TrimEnd);
        context.SetValue("table", Table);
        context.SetValue("dateAsText", functions.GetDatePhraseFunction());
        context.SetValue("urlPath", functions.GetUrlPathFunction());
        context.SetValue("appUrl", functions.GetAppUrlFunction());
        context.SetValue("localize", functions.GetLocalizeFunction());
    }
}