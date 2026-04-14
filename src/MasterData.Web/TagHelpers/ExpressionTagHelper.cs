using System.ComponentModel;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace JJMasterData.Web.TagHelpers;

public class ExpressionTagHelper : TagHelper
{
    [HtmlAttributeName("for")]
    public ModelExpression? For { get; set; }

    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    [HtmlAttributeName("tooltip")]
    [Localizable(false)]
    public string? Tooltip { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    [HtmlAttributeName("use-floating-label")]
    public bool UseFloatingLabel { get; set; } = true;

    [HtmlAttributeName("icon")]
    public FontAwesomeIcon? Icon { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var name = For?.Name ?? Name ?? throw new ArgumentException("For or Name properties are required.");

        string? modelValue = null;

        if (For is { Model: not null } && !string.IsNullOrWhiteSpace(For.Model.ToString()))
        {
            modelValue = For.Model.ToString();
        }
        else if (!string.IsNullOrWhiteSpace(Value))
        {
            modelValue = Value;
        }

        var isInvalid = ViewContext.ModelState[name]?.Errors.Count > 0;

        var fieldSet = new HtmlBuilder(HtmlTag.Fieldset);
        fieldSet.WithAttributeIf(Disabled, "disabled");

        var displayName = For?.ModelExplorer.Metadata.GetDisplayName() ?? Label;

        var label = new HtmlBuilder(HtmlTag.Label);
        label.WithAttribute("for", name + "-ExpressionValue");
        
        if (Icon.HasValue)
        {
            label.Append(HtmlTag.I, i =>
            {
                i.WithCssClass($"{Icon.Value.CssClass} me-1");
            });
        }
        
        label.AppendText(displayName);

        if (!string.IsNullOrEmpty(Tooltip))
        {
            label.AppendSpan(span =>
            {
                span.WithCssClass("fa-solid fa-circle-question help-description ms-1");
                span.WithToolTip(Tooltip);
            });
        }

        if (!UseFloatingLabel)
        {
            label.WithCssClass("form-label");
            fieldSet.Append(label);
        }

        fieldSet.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("mb-3");

            var editor = GetEditorHtml(name, modelValue);

            editor.WithAttributeIf(UseFloatingLabel, "placeholder", displayName!);
            editor.WithCssClassIf(isInvalid, "form-control is-invalid");
            editor.WithAttribute("id", name + "-ExpressionValue");

            if (UseFloatingLabel)
            {
                var formFloating = new HtmlBuilder(HtmlTag.Div);
                formFloating.WithCssClass("form-floating");
                formFloating.Append(editor);
                formFloating.Append(label);
                div.Append(formFloating);
            }
            else
            {
                div.Append(editor);
            }
        });

        output.TagMode = TagMode.StartTagAndEndTag;
        output.Content.SetHtmlContent(fieldSet);
    }

    private static HtmlBuilder GetEditorHtml(string name, string? value)
    {
        var input = HtmlBuilder.Input();
        input.WithCssClass("font-monospace form-control");
        input.WithNameAndId(name);
        input.WithAttribute("placeholder", string.Empty);

        if (value is null)
            return input;

        input.WithValue(value);

        return input;
    }
}