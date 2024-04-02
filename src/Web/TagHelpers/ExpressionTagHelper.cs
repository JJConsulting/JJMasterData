using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.UI;
using JJMasterData.Web.Configuration.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.TagHelpers;

public class ExpressionTagHelper(
    IEnumerable<IExpressionProvider> expressionProviders,
    IOptionsSnapshot<MasterDataWebOptions> options,
    IStringLocalizer<MasterDataResources> stringLocalizer) : TagHelper
{
    private bool? _isSyncExpression;

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

    [ViewContext] [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;

    [HtmlAttributeName("disabled")] 
    public bool Disabled { get; set; }

    private bool IsSyncExpression
    {
        get
        {
            _isSyncExpression ??= For
                ?.Metadata
                ?.ContainerType
                ?.GetProperty(For.Metadata.PropertyName!)
                ?.IsDefined(typeof(SyncExpressionAttribute), inherit: true) is true;

            return _isSyncExpression.Value;
        }
    }

    [HtmlAttributeName("icon")] public IconType? Icon { get; set; }

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

        var isInvalid = ViewContext.ModelState[name]?.Errors.Any() ?? false;

        var splittedExpression = modelValue?.Split(':', 2);

        string? selectedExpressionType = null;
        string? selectedExpressionValue = modelValue;

        if (splittedExpression?.Length == 2)
        {
            selectedExpressionType = splittedExpression[0];
            selectedExpressionValue = splittedExpression[1];
        }

        var fieldSet = new FieldSet();
        fieldSet.WithAttributeIf(Disabled, "disabled");

        var displayName = For?.ModelExplorer.Metadata.GetDisplayName() ?? Label;

        fieldSet.AppendIf(displayName is not null, HtmlTag.Div, div =>
        {
            var label = new Label();
            label.WithAttribute("for", name + "-ExpressionValue");
            label.AppendText(displayName!);
            var isAdvanced = options.Value.UseAdvancedModeAtExpressions;
            if (!isAdvanced)
            {
                div.WithCssClass("input-group");
                div.Append(GetTypeSelect(name, selectedExpressionType)
                    .WithCssClassIf(isInvalid, "form-select is-invalid")
                    .WithAttribute("id", name + "-ExpressionValue"));
            }

            div.AppendDiv(div =>
                {
                    div.WithCssClass("form-floating");
                    div.WithAttributeIf(!isAdvanced, "style", "width:75%");
                    div.Append(GetEditorHtml(name, selectedExpressionType, selectedExpressionValue));
                    div.Append(label);
                }).WithAttribute("placeholder", displayName!)
                .WithCssClass("mb-3")
                .WithCssClassIf(isInvalid, "form-control is-invalid")
                .WithAttribute("id", name + "-ExpressionValue");
        });

        output.TagMode = TagMode.StartTagAndEndTag;

        output.Content.SetHtmlContent(fieldSet.ToString());
    }

    private HtmlBuilder GetTypeSelect(string name, string? selectedExpressionType)
    {
        var select = new HtmlBuilder(HtmlTag.Select);
        select.WithNameAndId(name + "-ExpressionType");
        select.WithCssClass("form-select");

        foreach (var provider in expressionProviders)
        {
            if (IsSyncExpression && provider is not ISyncExpressionProvider)
                continue;

            select.Append(HtmlTag.Option, option =>
            {
                if (selectedExpressionType == provider.Prefix)
                    option.WithAttribute("selected", "selected");

                option.WithValue(provider.Prefix);
                option.AppendText(stringLocalizer[provider.Title]);
            });
        }

        return select;
    }

    private HtmlBuilder GetEditorHtml(string name,
        string? selectedExpressionType,
        string? selectedExpressionValue)
    {
        var advanced = options.Value.UseAdvancedModeAtExpressions;
        var input = new Input();
        input.WithCssClass("font-monospace");
        input.WithCssClass("form-control");
        input.WithNameAndId(name + "-ExpressionValue");
        input.WithAttribute("placeholder", string.Empty);
        if (selectedExpressionType is null)
            return input;

        var value = advanced
            ? selectedExpressionType + ":" + selectedExpressionValue
            : selectedExpressionValue;

        input.WithValue(value ?? string.Empty);

        if (!string.IsNullOrEmpty(Tooltip))
            input.WithToolTip(Tooltip);

        return input;
    }
}