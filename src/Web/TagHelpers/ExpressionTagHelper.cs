using System.Web;
using JJMasterData.Commons.Localization;
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
    private IEnumerable<IExpressionProvider> ExpressionProviders { get; }
    private readonly IStringLocalizer<JJMasterDataResources> _stringLocalizer;
    private readonly IComponentFactory<JJCard> _cardFactory;

    [HtmlAttributeName("for")]
    public ModelExpression For { get; set; } = null!;

    [HtmlAttributeName("tooltip")]
    public string? Tooltip { get; set; }


    [ViewContext]
    [HtmlAttributeNotBound] 
    public ViewContext ViewContext { get; set; } = null!;
    
    public ExpressionTagHelper(
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IEnumerable<IExpressionProvider> expressionProviders,
        ExpressionParser expressionParser,
        IComponentFactory<JJCard> cardFactory)
    {
        ExpressionProviders = expressionProviders;
        _stringLocalizer = stringLocalizer;
        _cardFactory = cardFactory;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var name = For.Name;
        var value = For.Model?.ToString();
        var selectedExpressionType = value?.Split(':')[0];
        var selectedExpressionValue = value?.Split(':')[1]  ?? string.Empty;
        
        var card = _cardFactory.Create();
        card.Layout = PanelLayout.Collapse;
        card.Title = _stringLocalizer[name];
        card.HtmlBuilderContent.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("row");
            div.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("col-sm-2");
                div.Append(HtmlTag.Label, label =>
                {
                    label.WithCssClass("form-label");
                    label.WithAttribute("for", name + "-ExpressionType");
                    label.AppendText("Type");
                });
                
                div.Append(HtmlTag.Select, select =>
                {
                    select.WithNameAndId( name + "-ExpressionType");
                    select.WithCssClass("form-select");

                    foreach (var provider in ExpressionProviders.Select(s => s.Prefix))
                    {
                        select.Append(HtmlTag.Option, option =>
                        {
                            if (selectedExpressionType == provider)
                            {
                                option.WithAttribute("selected", "selected");
                            }
                            option.AppendText(provider);
                        });
                    }
                });
            });

            div.Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("col-sm-10");
                div.Append(HtmlTag.Label, label =>
                {
                    label.WithCssClass("form-label");
                    label.WithAttribute("for", name + "-ExpressionValue");
                    label.AppendText("Expression");
                });
                
                if (Tooltip is not null)
                {
                    var icon = new JJIcon(IconType.QuestionCircle);
                    icon.CssClass += " help-description";
                    icon.Attributes["title"] = Tooltip;
                    icon.Attributes[BootstrapHelper.DataToggle] = "tooltip";
                    div.AppendComponent(icon);
                }

                if (selectedExpressionType == "sql")
                {
                    div.Append(HtmlTag.TextArea, textArea =>
                    {
                        textArea.WithNameAndId( name + "-ExpressionValue");
                        textArea.AppendText(selectedExpressionValue);
                    });

                    div.AppendScript(
                        $"onDOMReady(()=>{{CodeMirrorWrapper.setupCodeMirror('{name}-ExpressionValue',{{mode: 'text/x-sql',singleLine:true, hintList: {ViewContext.ViewBag.CodeMirrorHintList}, hintKey: '{{'}});}})");
                }
                else
                {   
                    div.Append(HtmlTag.Input, input =>
                    {
                        input.WithNameAndId( name + "-ExpressionValue");
                        input.WithValue(selectedExpressionValue);
                        input.WithCssClass("form-control");
                    });
                    
                }
                
            });
        });
        
                
        output.TagMode = TagMode.StartTagAndEndTag;
        
        output.Content.SetHtmlContent(card.GetHtml());
        output.PostElement.SetHtmlContent(@$"
    <script>
    document.getElementById('{name}-ExpressionType').addEventListener('change', function () {{
        const selectedType = this.value;
        const expressionValueInput = document.getElementById('{name}-ExpressionValue');
        if (selectedType === 'sql') {{
            const textArea = document.createElement('textarea');
            textArea.setAttribute('name', '{name}-ExpressionValue');
            textArea.setAttribute('id', '{name}-ExpressionValue');
            textArea.setAttribute('class', 'form-control');
            textArea.innerText = expressionValueInput.value;
            expressionValueInput.outerHTML = textArea.outerHTML;
            CodeMirrorWrapper.setupCodeMirror('{name}-ExpressionValue', {{ mode: 'text/x-sql',singleLine:true, hintList: " + ViewContext.ViewBag.CodeMirrorHintList + @$", hintKey: '{{' }});
        }} else {{
            const input = document.createElement('input');
            input.setAttribute('type', 'text');
            input.setAttribute('class', 'form-control');
            input.setAttribute('name', '{name}-ExpressionValue');
            input.setAttribute('id', '{name}-ExpressionValue');
            input.value = expressionValueInput.value;

            if(expressionValueInput.codeMirrorInstance){{
                expressionValueInput.codeMirrorInstance.setOption('mode', 'text/x-csrc');
                expressionValueInput.codeMirrorInstance.getWrapperElement().parentNode.removeChild(expressionValueInput.codeMirrorInstance.getWrapperElement());
            }}

            expressionValueInput.outerHTML = input.outerHTML;
           
        }}
    }});
    </script>
");
    }
    
}