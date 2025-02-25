#nullable disable


using JJMasterData.Web.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

[HtmlTargetElement("code-editor", TagStructure = TagStructure.WithoutEndTag)]
public sealed class CodeEditorTagHelper(ComponentRenderer componentRenderer) : TagHelper
{
    [HtmlAttributeName("asp-for")]
    public required ModelExpression For { get; set; }
    
    [HtmlAttributeName("language")]
    public required string Language { get; set; }

    [HtmlAttributeName("height")]
    public int Height { get; set; } = 500;
    
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    { 
        var parameters =  new Dictionary<string, object>
        {
            {"Name", For.Name},
            {"Value", For.Model},
            {"Language", Language},
            {"Height", Height}
        };
        var content = await componentRenderer.RenderAsync<CodeEditor>(parameters);
        output.TagName = null;
        output.Content.SetHtmlContent(content);
    }
}