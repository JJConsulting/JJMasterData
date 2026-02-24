#nullable disable

using JJConsulting.Html.Bootstrap.TagHelpers.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

[HtmlTargetElement("code-editor", TagStructure = TagStructure.WithoutEndTag)]
public sealed class CodeEditorTagHelper(IControlFactory<JJCodeEditor> codeEditorFactory, IHtmlHelper htmlHelper) : TagHelper
{
    [HtmlAttributeName("name")]
    public string Name { get; set; }
    
    [HtmlAttributeName("value")]
    public string Value { get; set; }
    
    [HtmlAttributeName("asp-for")]
    public required ModelExpression For { get; set; }
    
    [HtmlAttributeName("language")]
    public required string Language { get; set; }

    [HtmlAttributeName("height")]
    public int Height { get; set; } = 500;
    
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }
    
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;
    
    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        Contextualize();
        
        var name = string.IsNullOrEmpty(Name) ? htmlHelper.Name(For!.Name) : Name;
        var value = string.IsNullOrEmpty(Value) ? For?.Model : Value;

        var codeEditor = codeEditorFactory.Create();
        codeEditor.Name = name;
        codeEditor.Height = Height;
        codeEditor.Language = Language;
        codeEditor.Text = value?.ToString();
        codeEditor.Enabled = !Disabled;
        
        output.TagName = null;
        output.Content.SetHtmlContent(codeEditor.GetHtmlBuilder());
    }
    
    private void Contextualize()
    {
        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(ViewContext);
        }
    }
}