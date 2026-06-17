#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Html.Templates;

[PublicAPI]
public class HtmlTemplateRenderer(
    FluidParser fluidParser,
    IServiceProvider serviceProvider)
{
    public IFluidTemplate ParseTemplate(string template)
    {
        return fluidParser.Parse(template);
    }
    
    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
            return new(error);

        var context = new HtmlTemplateContext(serviceProvider, values);

        return template.RenderAsync(context, HtmlEncoder.Default);
    }
    
    public ValueTask<string> RenderTemplate(string templateString, TemplateContext context)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
            return new(error);

        return template.RenderAsync(context, HtmlEncoder.Default);
    }

    public ValueTask<string> RenderTemplate(IFluidTemplate template, TemplateContext context)
    {
        context.AmbientValues[HtmlTemplateOptions.ServiceProviderKey] = serviceProvider;
        
        return template.RenderAsync(context, HtmlEncoder.Default);
    }

    public ValueTask<string> RenderTemplate(FormElement formElement, FormElementField field, Dictionary<string, object> values)
    {
        var context = new HtmlTemplateContext(serviceProvider, values)
        {
            AmbientValues =
            {
                [HtmlTemplateOptions.FormElementKey] = formElement,
                [HtmlTemplateOptions.FormElementFieldKey] = field,
                [HtmlTemplateOptions.FormValuesKey] = values
            }
        };
        
        return RenderTemplate(field.GridRenderingTemplate, context);
    }
}
