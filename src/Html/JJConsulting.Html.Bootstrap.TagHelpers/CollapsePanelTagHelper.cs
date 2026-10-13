using JetBrains.Annotations;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJConsulting.Html.Bootstrap.TagHelpers;

public class CollapsePanelTagHelper : TagHelper
{
    [HtmlAttributeName("name")] public string? Name { get; set; }

    [HtmlAttributeName("title")]
    [LocalizationRequired]
    public string? Title { get; set; }

    [HtmlAttributeName("icon")]
    public FontAwesomeIcon Icon { get; set; }

    [HtmlAttributeName("expanded-by-default")]
    public bool ExpandedByDefault { get; set; }
    
    [HtmlAttributeName("color")]
    public BootstrapColor Color { get; set; }

    [HtmlAttributeName("visible")]
    public bool Visible { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        AssertAttributes();

        var panel = new JJCollapsePanel
        {
            Name = Name ?? Title!.ToLowerInvariant().Replace(" ", "_"),
            Title = Title,
            ExpandedByDefault = ExpandedByDefault,
            Color = Color,
            Visible = Visible
        };

        if (Icon != default)
            panel.TitleIcon = new JJIcon(Icon);

        var content = await output.GetChildContentAsync();
        panel.Content = content;
        
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(panel);
    }

    private void AssertAttributes()
    {
        if (Title == null)
            throw new ArgumentNullException(nameof(Title));
    }
}