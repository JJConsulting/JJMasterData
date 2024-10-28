using JetBrains.Annotations;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class MasterDataFormTagHelper(IHtmlGenerator generator) : FormTagHelper(generator)
{
    [HtmlAttributeName("asp-action")]
    [AspMvcAction]
    public string AspMvcAction
    {
        get => Action;
        set => Action = value;
    }
    
    [HtmlAttributeName("asp-controller")]
    [AspMvcController]
    public string AspMvcController
    {
        get => Controller;
        set => Controller = value;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.TagName = "form";
        output.Attributes.SetAttribute("id",HtmlHelperExtensions.MasterDataFormId);
    }
}