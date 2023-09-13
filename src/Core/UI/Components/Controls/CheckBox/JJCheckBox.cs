using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJCheckBox : ControlBase
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
            if (_isChecked == null && Request.IsPost)
                _isChecked = Value.Equals(Request[Name]);

            return _isChecked ?? false;
        }
        set => _isChecked = value;
    }

    public JJCheckBox(IHttpRequest httpRequest) : base(httpRequest)
    {
        Visible = true;
        Enabled = true;
        Value = "1";
    }


    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.Version == 3 ? "form-check" : "checkbox")
            .WithCssClassIf(!Enabled, "disabled")
            .Append(GetInputHtml());

        var result = new RenderedComponentResult(html);
        
        return await Task.FromResult(result);
        
    }

    private HtmlBuilder GetInputHtml()
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        div.WithCssClass("form-check");
        
        div.Append(HtmlTag.Input, input =>
        {
            input.WithAttributes(Attributes)
                .WithAttribute("type", "checkbox")
                .WithNameAndId(Name)
                .WithAttribute("value", Value)
                .WithCssClass("form-check-input")
                .WithCssClass(CssClass)
                .WithAttribute("onchange",$"$('#{Name}_hidden').val($(this).is(':checked') ? '{Value}' : '0');")
                .WithToolTip(Tooltip)
                .WithAttributeIf(IsChecked, "checked", "checked")
                .WithAttributeIf(!Enabled, "disabled", "disabled");
        });

        div.AppendHiddenInput($"{Name}_hidden", IsChecked ? Value : "0");

        div.AppendIf(!string.IsNullOrEmpty(Text), HtmlTag.Label, label =>
        {
            label.WithAttribute("for", Name);
            label.AppendText(Text);
        });

        return div;
    }
}
