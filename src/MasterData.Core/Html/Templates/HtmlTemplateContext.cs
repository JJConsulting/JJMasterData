using System;
using System.Collections.Generic;
using Fluid;

namespace JJMasterData.Core.Html.Templates;

internal sealed class HtmlTemplateContext : TemplateContext
{
    public HtmlTemplateContext(IServiceProvider serviceProvider, Dictionary<string, object> values) : base(values, HtmlTemplateOptions.Value, allowModelMembers: true)
    {
        AmbientValues[HtmlTemplateOptions.ServiceProviderKey] = serviceProvider;
    }
}