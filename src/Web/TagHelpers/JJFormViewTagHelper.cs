using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJFormViewTagHelper : TagHelper
{
    private FormViewFactory FormViewFactory { get; }

    [HtmlAttributeName("element-name")] 
    public string? ElementName { get; set; }

    [HtmlAttributeName("form-element")]
    public FormElement? FormElement { get; set; }

    [HtmlAttributeName("configure")] 
    public Action<JJFormView>? Configure { get; set; }

    public JJFormViewTagHelper(FormViewFactory formViewFactory)
    {
        FormViewFactory = formViewFactory;
    }
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        JJFormView formView;

        if(ElementName is not null)
        {
            formView = await FormViewFactory.CreateFormViewAsync(ElementName);
        }
        else if(FormElement is not null)
        {
            formView = FormViewFactory.CreateFormView(FormElement);
        }
        else
        {
            throw new InvalidOperationException("Please set ElementName or FormElement at your JJFormView TagHelper.");
        }

        Configure?.Invoke(formView);
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(await formView.GetHtmlAsync());
    }
}