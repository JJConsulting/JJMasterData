#nullable disable

using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.UI.Components;
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

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
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
        
        var modal = new JJModalDialog(); 
        modal.Title = Label;
        modal.Name = $"{name}_modal";
        modal.Content = modalContent;
        
        var btnOk = new JJLinkButton();
        btnOk.Text = stringLocalizer["Ok"];
        btnOk.IconClass = "fa fa-check";
        btnOk.ShowAsButton = true;
        btnOk.CssClass = "btn btn-primary";
        btnOk.Attributes.Add("data-bs-dismiss", "modal");
        modal.Buttons.Add(btnOk);


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
        if (!string.IsNullOrEmpty(value))
        {
            badge.AppendSpan(s =>
                s.WithCssClass("position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger")
                    .AppendSpan(ico => ico.WithCssClass("fa-solid fa-bolt"))
            );    
        }
        
        var btn = new JJLinkButton();
        btn.Name = $"{name}_btn";
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
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass($"col-sm-12");
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
