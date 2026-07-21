#nullable disable

using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Web.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.TagHelpers;

[HtmlTargetElement("code-editor-button", Attributes = "asp-for,label")]
public sealed class CodeEditorButtonTagHelper(
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IControlFactory<JJCodeEditor> codeEditorFactory, 
    IHtmlHelper htmlHelper) : TagHelper
{

    [HtmlAttributeName("asp-for")]
    public required ModelExpression For { get; set; }

    [HtmlAttributeName("label")]
    public required string Label { get; set; }

    [HtmlAttributeName("language")]
    public string Language { get; set; } = "plaintext";

    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    [HtmlAttributeName("required")]
    public bool Required { get; set; }
    
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        Contextualize();

        var name = htmlHelper.Name(For.Name);
        
        var codeEditor = codeEditorFactory.Create();
        codeEditor.Name = name;
        codeEditor.Language = Language;
        codeEditor.Text = For.Model?.ToString();
        codeEditor.Enabled = !Disabled;
        codeEditor.CursorPosition = 0;

        var modalContent = new HtmlBuilder();
        modalContent.Append(GetHtmlTextInfo());
        modalContent.Append(codeEditor.GetHtmlBuilder());
        
        var tagHelperContent = await output.GetChildContentAsync();

        if (!tagHelperContent.IsEmptyOrWhiteSpace)
        {
            modalContent.Append(tagHelperContent);
        }
        
        var btnOk = new JJLinkButton
        {
            Text = stringLocalizer["Ok"],
            IconClass = "fa fa-check",
            ShowAsButton = true,
            CssClass = "btn btn-primary",
            Attributes =
            {
                ["data-bs-dismiss"] = "modal"
            }
        };
        
        var modal = new JJModalDialog
        {
            Title = Label,
            Name = $"{name}_modal",
            Size = ModalSize.Large,
            Content = modalContent,
            Buttons = [btnOk]
        };

        var html = new HtmlBuilder();
        html.Append(modal);
        
        var btn = GetButton();
        html.Append(btn);
        
        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }

    private JJLinkButton GetButton()
    {
        var name = htmlHelper.Name(For.Name);
        var value = For.Model?.ToString();
        
        var badge = new HtmlBuilder();
        badge.AppendText(" ");
        badge.AppendText(Label);

        var isEmpty = string.IsNullOrEmpty(value);
        
        if (!isEmpty)
        {
            badge.AppendSpan(s =>
                s.WithCssClass("position-absolute top-0 start-100 translate-middle badge rounded-pill bg-success")
                    .AppendSpan(icon => icon.WithCssClass("fa fa-check"))
            );    
        }
        else if (Required && !Disabled)
        {
            badge.AppendSpan(s =>
                s.WithCssClass("position-absolute top-0 start-100 translate-middle badge rounded-pill bg-warning")
                    .AppendSpan(icon => icon.WithCssClass("fa fa-exclamation-triangle"))
                    .WithToolTip(stringLocalizer["Required field"])
            );
        }
        
        var btn = new JJLinkButton
        {
            Name = $"{name}_btn"
        };
        btn.InnerHtml.Append(badge);
        btn.ShowAsButton = true;
        btn.CssClass = "position-relative";
        btn.Attributes.Add("data-bs-toggle", "modal");
        btn.Attributes.Add("data-bs-target", $"#{name}_modal");
        
        if ("sql".Equals(Language, StringComparison.OrdinalIgnoreCase))
            btn.IconClass = "fa-solid fa-database";
        else
            btn.IconClass = "fa-solid fa-code";
        
        return btn;
    }
    
    private HtmlBuilder GetHtmlTextInfo()
    {
        var div = HtmlBuilder.Div();
        div.WithCssClass("col-sm-12");
        div.AppendSpan(s => 
            s.WithCssClass("small text-info")
             .AppendText(stringLocalizer["(Type Ctrl+Space to autocomplete)"])
        );
        return div;
    }
    
    private void Contextualize()
    {
        if (htmlHelper is IViewContextAware aware)
        {
            aware.Contextualize(ViewContext);
        }
    }
}
