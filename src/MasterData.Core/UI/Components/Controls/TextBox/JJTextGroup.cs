using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;


namespace JJMasterData.Core.UI.Components;

public sealed class JJTextGroup(IFormValues formValues)
    : JJTextBox(formValues), IFloatingLabelControl
{
    public string FloatingLabel { get; set; }

    public bool UseFloatingLabel { get; set; }

    /// <summary>
    /// Actions of input
    /// </summary>
    public List<JJLinkButton> Actions { get; } = [];

    /// <summary>
    /// Text info on left of component
    /// </summary>
    public InputAddons Addons { get; set; }
    
    public string GroupCssClass { get; set; }

    protected internal override ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = GetHtmlBuilder();

        return html.AsValueTask();
    }

    public override HtmlBuilder GetHtmlBuilder()
    {
        var defaultAction = Actions.Find(x => x.IsDefaultOption && x.Visible);
        if (!Enabled)
        {
            if (defaultAction != null)
            {
                ReadOnly = true;
                Enabled = true;
            }
        }

        var input = base.GetHtmlBuilder();
        var hasAction = Actions.Exists(x => x.Visible);
        var hasAddons = Addons != null;
        if (InputType == InputType.Phone)
        {
            Attributes.TryGetValue(FormElementField.DefaultFormatAttribute,  out var defaultCountry);
            var optContentTpl = "<span class='fi fi-{0}'></span><span>&nbsp;{1}</span>";
            var dropDownGroup = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass("input-group jjform-action ")
                .WithCssClass(GroupCssClass)
                // .AppendButton(button =>
                // {
                //     button.WithCssClass("btn btn-secondary dropdown-toggle ");
                //     button.WithAttribute(BootstrapHelper.DataToggle, "dropdown");
                //     button.WithAttribute("type", "button");
                // })
                .AppendSelect(select =>
                {
                    select.WithCssClass("selectpicker w-auto jj-phone-select");
                    select.WithAttribute("data-live-search", "true");
                    select.WithAttribute("data-style-base", "form-select form-dropdown");
                    foreach (CountryInfo countryInfo in CountryHelper.All)
                    {
                        select.AppendOption(opt =>
                        {
                            opt.WithAttributeIf(defaultCountry == countryInfo.Code, "selected", true.ToString());
                            opt.WithAttribute("dial-code", countryInfo.DialCode);
                            opt.WithValue(countryInfo.Code);
                            opt.WithAttribute("data-content", string.Format(optContentTpl, countryInfo.Code.ToLower(), countryInfo.DialCode));
                        });
                    }
                });
            input.WithCssClass("jj-phone-input");
            dropDownGroup.Append(input);
            return dropDownGroup;
        }
        if (!hasAction && !hasAddons)
        {

            if (UseFloatingLabel)
            {
                input.WithAttribute("placeholder");
                return new HtmlBuilder(HtmlTag.Div).WithCssClass("form-floating")
                    .Append(input)
                    .AppendLabel(label =>
                    {
                        label.AppendText(FloatingLabel);
                        label.WithAttribute("for", Name);
                    });
            }

            return input;
        }

        if (defaultAction is { Enabled: true })
        {
            input.WithCssClass("default-option");
            input.WithOnChange(defaultAction.OnClientClick);
        }

        var inputGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass);
        
        if (hasAddons )
            inputGroup.Append(GetHtmlAddons());

        if (UseFloatingLabel)
        {
            input.WithAttribute("placeholder");
            inputGroup.AppendDiv(div =>
            {
                div.WithCssClass("form-floating");
                div.Append(input);
                div.AppendLabel(label=>
                {
                    label.AppendText(FloatingLabel);
                    label.WithAttribute("for", Name);
                });
            });
        }
        else
        {
            inputGroup.Append(input);
        }

        if (hasAction)
            AddActionsAt(inputGroup);

        return inputGroup;
    }

    private void AddActionsAt(HtmlBuilder inputGroup)
    {
        HtmlBuilder builderGroup;
        if (BootstrapHelper.Version >= 5)
        {
            builderGroup = inputGroup;
        }
        else
        {
            builderGroup = new HtmlBuilder(HtmlTag.Div)
                .WithCssClass(BootstrapHelper.InputGroupBtn);

            inputGroup.Append(builderGroup);
        }

        var btnGroup = new JJLinkButtonGroup
        {
            Actions = Actions
        };

        if (UseFloatingLabel)
            Actions.ForEach(a => a.CssClass += "btn-floating-action");

        btnGroup.ShowAsButton = true;

        //Add builder Actions
        btnGroup.AddActionsAt(builderGroup);
    }
    
    private HtmlBuilder GetHtmlAddons()
    {
        var html = new HtmlBuilder(HtmlTag.Span)
            .WithCssClass(BootstrapHelper.InputGroupAddon)
            .WithToolTip(Addons.Tooltip)
            .AppendIf(Addons.Icon != null, () => Addons.Icon.GetHtmlBuilder())
            .AppendTextIf(!string.IsNullOrEmpty(Addons.Text), Addons.Text);

        return html;
    }
}