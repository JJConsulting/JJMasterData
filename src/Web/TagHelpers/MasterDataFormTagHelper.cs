using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class MasterDataFormTagHelper(IHtmlGenerator generator) : FormTagHelper(generator)
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        base.Process(context, output);
        output.TagName = "form";
        output.Attributes.SetAttribute("id",HtmlHelperExtensions.MasterDataFormId);
    }
}