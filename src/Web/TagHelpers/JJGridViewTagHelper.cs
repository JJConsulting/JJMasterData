using System.Collections;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class JJGridViewTagHelper : TagHelper
{

    [HtmlAttributeName("element-name")]
    public string? ElementName { get; set; }

    [HtmlAttributeName("form-element")]
    public FormElement? FormElement { get; set; }

    [HtmlAttributeName("datatable")]
    public DataTable? DataTable { get; set; }

    [HtmlAttributeName("configure")]
    public Action<JJGridView>? Configure { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {

        JJGridView gridView;

        if (ElementName is not null)
        {
            gridView = WebComponentFactory.CreateGridView(ElementName);
        }
        else if (FormElement is not null)
        {
            gridView = WebComponentFactory.CreateGridView(FormElement);
        }
        else if (DataTable is not null)
        {
            gridView = WebComponentFactory.CreateGridView(DataTable);
        }
        else
        {
            throw new InvalidOperationException("Please set ElementName, FormElement, DataTable at your JJGridView TagHelper.");
        }

        Configure?.Invoke(gridView);

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(gridView.GetHtml());
        return Task.CompletedTask;
    }
}