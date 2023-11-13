using System.ComponentModel;
using System.Web;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.TagHelpers;

public class ExpressionTagHelper : TagHelper
{
    private readonly IEnumerable<IExpressionProvider> _expressionProviders;
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer;
    private readonly IComponentFactory<JJCard> _cardFactory;
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
    
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;
    
    
    public ExpressionTagHelper(
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEnumerable<IExpressionProvider> expressionProviders,
        ExpressionParser expressionParser,
        IComponentFactory<JJCard> cardFactory)
    {
        _expressionProviders = expressionProviders;
        _stringLocalizer = stringLocalizer;
        _cardFactory = cardFactory;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var name = For?.Name ?? Name ?? throw new ArgumentException("For or Name properties are required.");
        var modelValue = For?.Model?.ToString() ?? Value;
        var selectedExpressionType = modelValue?.Split(':')[0];
        var selectedExpressionValue = modelValue?.Split(':')[1]  ?? string.Empty;
        string codeMirrorHintList = ViewContext.ViewBag.CodeMirrorHintList;
        
        var card = _cardFactory.Create();
        card.Layout = PanelLayout.Collapse;
        card.Icon = Icon;
        card.Title = For?.ModelExplorer.Metadata.GetDisplayName();
        card.HtmlBuilderContent.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("row");
            div.Append(GetTypeSelect(name, selectedExpressionType));

            div.Append(GetEditorHtml(name, selectedExpressionType, selectedExpressionValue, codeMirrorHintList));
        });

        var html = card.GetHtmlBuilder();

        html.AppendScript($"listenExpressionType('{name}',{codeMirrorHintList}, {(IsBooleanExpression ? "true" : "false")})");
        
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Content.SetHtmlContent(html.ToString());
    }

    private HtmlBuilder GetTypeSelect(string? name, string? selectedExpressionType)
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("col-sm-2");
        div.Append(HtmlTag.Label, label =>
        {
            label.WithCssClass("form-label");
            label.WithAttribute("for", name + "-ExpressionType");
            label.AppendText(_stringLocalizer["Type"]);
        });

        div.Append(HtmlTag.Select, select =>
        {
            select.WithNameAndId(name + "-ExpressionType");
            select.WithCssClass("form-select");
            
            foreach (var provider in _expressionProviders)
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

    private HtmlBuilder GetEditorHtml(string name, string? selectedExpressionType, string selectedExpressionValue, string codeMirrorHintList)
    {
        var div = new HtmlBuilder(HtmlTag.Div);
      
            div.WithCssClass("col-sm-10");
            div.Append(HtmlTag.Label, label =>
            {
                label.WithCssClass("form-label");
                label.WithAttribute("for", name + "-ExpressionValue");
                label.AppendText(_stringLocalizer["Expression"]);
            });

            if (Tooltip is not null)
            {
                var icon = new JJIcon(IconType.QuestionCircle);
                icon.CssClass += " help-description";
                icon.Attributes["title"] = _stringLocalizer[Tooltip];
                icon.Attributes[BootstrapHelper.DataToggle] = "tooltip";
                div.AppendComponent(icon);
            }

            div.Append(HtmlTag.Div, div =>
            {
                div.WithId(name + "-ExpressionValueEditor");
                
                if ((selectedExpressionType == "val" || string.IsNullOrEmpty(selectedExpressionType)) && IsBooleanExpression)
                {
                    div.Append(HtmlTag.Div, div =>
                    {
                        div.WithCssClass("form-switch form-switch-md form-check");
                        div.Append(HtmlTag.Input, input =>
                        {
                            input.WithAttribute("hidden","hidden");
                            input.WithNameAndId(name + "-ExpressionValue");
                            input.WithValue(selectedExpressionValue);
                        });
                        div.Append(HtmlTag.Input, checkbox =>
                        {
                            checkbox.WithNameAndId(name + "-ExpressionValue-checkbox");
                            checkbox.WithAttribute("type", "checkbox");
                            checkbox.WithAttribute("role", "switch");
                            checkbox.WithAttribute("onchange", $"CheckboxHelper.check('{name + "-ExpressionValue"}')");
                            checkbox.WithAttributeIf(StringManager.ParseBool(selectedExpressionValue) ,"checked");
                            checkbox.WithValue(selectedExpressionValue);
                            checkbox.WithCssClass("form-check-input");
                        });
                        
                    });
                }
                else
                {
                    div.Append(HtmlTag.TextArea, textArea =>
                    {
                        textArea.WithNameAndId(name + "-ExpressionValue");
                        textArea.AppendText(selectedExpressionValue);
                    });

                    div.AppendScript(
                        @$"onDOMReady(()=>{{CodeMirrorWrapper.setupCodeMirror('{name}-ExpressionValue',{{mode: 'text/x-sql',singleLine:true, hintList: {codeMirrorHintList}, hintKey: '{{'}});}})");
                }
            });
            return div;
    }
}