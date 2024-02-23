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

    [ViewContext] 
    [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;
    
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    private bool IsSqlExpression
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

    [HtmlAttributeName("icon")]
    public IconType? Icon { get; set; }

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

        var selectedExpressionType =expressionProviders.First(p=> p is ValueExpressionProvider).Prefix;
        var selectedExpressionValue = modelValue ?? string.Empty;
        if (splittedExpression?.Length == 2)
        {
            selectedExpressionType = splittedExpression[0];
            selectedExpressionValue = splittedExpression[1];
        }
        
        var fieldSet = new HtmlBuilder(HtmlTag.FieldSet);
        fieldSet.WithAttributeIf(Disabled, "disabled");
        var displayName = For?.ModelExplorer.Metadata.GetDisplayName() ?? Label;
        fieldSet.AppendIf(displayName is not null, HtmlTag.Label, label =>
        {
            label.WithCssClass(BootstrapHelper.Label);
            label.WithAttribute("for", name + "-ExpressionValue");
            label.AppendText(displayName!);
            if (!string.IsNullOrWhiteSpace(Tooltip))
            {
                label.AppendSpan(span =>
                {
                    span.WithCssClass("fa fa-question-circle help-description");
                    span.WithToolTip(Tooltip);
                });
            }
        });

        fieldSet.AppendDiv(div =>
        {
            if (!options.Value.UseAdvancedModeAtExpressions)
            {
                div.WithCssClass("input-group");
                div.Append(GetTypeSelect(name, selectedExpressionType)
                    .WithCssClassIf(isInvalid,"is-invalid"));
            }
            div.Append(GetEditorHtml(name, selectedExpressionType, selectedExpressionValue)
                .WithCssClassIf(isInvalid,"is-invalid"));
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
            if (IsSqlExpression && provider is not ISyncExpressionProvider)
                continue;

            select.Append(HtmlTag.Option, option =>
            {
                if (selectedExpressionType == provider.Prefix)
                {
                    option.WithAttribute("selected", "selected");
                }

                option.WithValue(provider.Prefix);
                option.AppendText(stringLocalizer[provider.Title]);
            });
        }

        return select;
    }

    private HtmlBuilder GetEditorHtml(string name, string? selectedExpressionType,
        string selectedExpressionValue)
    {
        var advanced = options.Value.UseAdvancedModeAtExpressions;
        var input = new HtmlBuilder(HtmlTag.Input);
        input.WithCssClass("font-monospace");
        input.WithCssClass("form-control");
        input.WithAttributeIf(!advanced,"style", "width:75%");
        input.WithNameAndId(name + "-ExpressionValue");
        var value = advanced ? selectedExpressionType + ":" + selectedExpressionValue : selectedExpressionValue;
        input.WithValue(value);
        return input;
    }
}