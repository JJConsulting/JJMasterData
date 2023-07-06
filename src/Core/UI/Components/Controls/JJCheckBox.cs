﻿using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJCheckBox : JJBaseControl
{
    private bool? _isChecked;

    /// <remarks>
    /// Default: 1
    /// </remarks>
    public string Value { get; set; }

    public new string Text { get; set;}

    public bool IsChecked
    {
        get
        {
            if (_isChecked == null && CurrentContext.IsPost)
                _isChecked = Value.Equals(CurrentContext.Request[Name]);

            return _isChecked ?? false;
        }
        set => _isChecked = value;
    }

    private static IExpressionsService ExpressionsService =>
        JJService.Provider.GetScopedDependentService<IExpressionsService>();
    
    public JJCheckBox(IHttpContext httpContext) : base(httpContext)
    {
        Visible = true;
        Enabled = true;
        Value = "1";
    }


    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .AppendElement(GetInputHtml());

        return html;
    }

    private HtmlBuilder GetInputHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClass("form-check");
        
        div.AppendElement(HtmlTag.Input, input =>
        {
            input.WithAttributes(Attributes)
                .WithAttribute("type", "checkbox")
                .WithNameAndId(Name)
                .WithAttribute("value", Value)
                .WithCssClass("form-check-input")
                .WithCssClass(CssClass)
                .WithAttribute("onchange",$"$('#{Name}_hidden').val($(this).is(':checked') ? '{Value}' : '0');")
                .WithToolTip(Translate.Key(ToolTip))
                .WithAttributeIf(IsChecked, "checked", "checked")
                .WithAttributeIf(!Enabled, "disabled", "disabled");
        });

        div.AppendHiddenInput(Name + "_hidden", IsChecked ? Value : "0");

        div.AppendElementIf(!string.IsNullOrEmpty(Text), HtmlTag.Label, label =>
        {
            label.WithAttribute("for", Name);
            label.AppendText(Text);
        });

        return div;
    }
    
}