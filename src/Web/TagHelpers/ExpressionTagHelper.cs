using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using JJMasterData.Core.UI;

namespace JJMasterData.Web.TagHelpers;

public class ExpressionTagHelper(IEnumerable<IExpressionProvider> expressionProviders) : TagHelper
{
    private bool? _isBooleanExpression;

    [HtmlAttributeName("for")]
    public ModelExpression? For { get; set; }
    
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("value")]
    public string? Value { get; set; }
    
    [HtmlAttributeName("title")]
    public string? Title { get; set; }
    
    [HtmlAttributeName("tooltip")]
    [Localizable(false)]
    public string? Tooltip { get; set; }
    
    private bool IsBooleanExpression
    {
        get
        {
            _isBooleanExpression ??= For
                ?.Metadata
                ?.ContainerType
                ?.GetProperty(For.Metadata.PropertyName!)
                ?.IsDefined(typeof(BooleanExpressionAttribute), inherit: true) is true;

            return _isBooleanExpression.Value;
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

        var splittedExpression = modelValue?.Split(':',2);
        var selectedExpressionType = splittedExpression?[0];
        var selectedExpressionValue = splittedExpression?[1] ?? string.Empty;

        var html = new HtmlBuilder(HtmlTag.Div);
        html.Append(HtmlTag.Label, label =>
        {
            label.WithCssClass(BootstrapHelper.Label);
            label.WithAttribute("for",name + "-ExpressionValue");
            label.AppendText(For?.ModelExplorer.Metadata.GetDisplayName()!);
            label.AppendSpan(span =>
            {
                span.WithCssClass("fa fa-question-circle help-description");
                span.WithToolTip(Tooltip);
            });
        });
        html.WithCssClass("row");
        html.Append(GetTypeSelect(name, selectedExpressionType));
        html.Append(GetEditorHtml(name, selectedExpressionType, selectedExpressionValue));
        
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Content.SetHtmlContent(html.ToString());
    }

    private HtmlBuilder GetTypeSelect(string? name, string? selectedExpressionType)
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("col-sm-2");
        div.Append(HtmlTag.Select, select =>
        {
            select.WithNameAndId(name + "-ExpressionType");
            select.WithCssClass("form-select");
            
            foreach (var provider in expressionProviders)
            {
                if (IsBooleanExpression && provider is not IBooleanExpressionProvider)
                    continue;
                
                select.Append(HtmlTag.Option, option =>
                {
                    if (selectedExpressionType == provider.Prefix)
                    {
                        option.WithAttribute("selected", "selected");
                    }

                    option.WithValue(provider.Prefix);
                    option.AppendText(provider.Title);
                });
            }
        });
        return div;
    }

    private static HtmlBuilder GetEditorHtml(string name, string? selectedExpressionType, string selectedExpressionValue)
    {
        var div = new HtmlBuilder(HtmlTag.Div);
            div.WithCssClass("col-sm-10");
            div.Append(HtmlTag.Div, div =>
            {
                div.WithId(name + "-ExpressionValueEditor");
                div.Append(HtmlTag.Input, input =>
                {
                    input.WithCssClass("font-monospace");
                    input.WithCssClass("form-control");
                    input.WithNameAndId(name + "-ExpressionValue");
                    input.WithValue(selectedExpressionValue);
                });
            });
            return div;
    }
}