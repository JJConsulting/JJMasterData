#nullable disable warnings
using System;
using System.Linq;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Web.Components.Phone;

public sealed class JJPhoneGroup(IHttpContextAccessor formValues) : JJTextBox(formValues)
{
    public string GroupCssClass { get; set; }

    public override HtmlBuilder GetHtmlBuilder()
    {
        Attributes.TryGetValue(FormElementField.DefaultFormatAttribute, out var defaultCountry);

        var input = base.GetHtmlBuilder();
        var selectedCountry = GetCountryFromPhoneValue(Text)
            ?? (CountryHelper.TryGet(defaultCountry, out var configuredCountry) ? configuredCountry : null);
        var phoneValue = Text ?? string.Empty;
        var localPhoneValue = GetLocalPhoneValue(phoneValue, selectedCountry);
        var phoneGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("jj-phone-group");

        var hiddenInput = new HtmlBuilder(HtmlTag.Input)
            .WithAttribute("type", "hidden")
            .WithName(Name)
            .WithId($"{Name}_hidden")
            .WithValue(phoneValue)
            .WithCssClass("jj-phone-hidden-input");

        var dropDownGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass)
            .AppendSelect(select =>
            {
                select.WithCssClass("form-select tom-select w-auto jj-phone-select");
                select.WithCssClassIf(!Enabled, "disabled");
                select.WithAttributeIf(!Enabled, "disabled");
                foreach (var countryInfo in CountryHelper.All)
                {
                    select.AppendOption(opt =>
                    {
                        opt.WithAttributeIf(selectedCountry?.Code == countryInfo.Code, "selected", true.ToString());
                        opt.WithAttribute("dial-code", countryInfo.DialCode);
                        opt.WithValue(countryInfo.Code);
                        opt.WithAttribute("data-content", GetOptionContent(countryInfo).ToString());
                        opt.AppendText($"{countryInfo.Name} {countryInfo.DialCode}");
                    });
                }
            });

        input.WithCssClass("jj-phone-input");
        input.WithAttribute("id", Name);
        input.WithAttribute("name", $"{Name}_display");
        input.WithAttribute("value", localPhoneValue);
        dropDownGroup.Append(input);

        phoneGroup.Append(hiddenInput);
        phoneGroup.Append(dropDownGroup);

        return phoneGroup;
    }

    private static CountryInfo GetCountryFromPhoneValue(string phoneValue)
    {
        if (string.IsNullOrWhiteSpace(phoneValue) || !phoneValue.StartsWith("+"))
            return null;

        return CountryHelper.All
            .OrderByDescending(c => NormalizeDialCode(c.DialCode).Length)
            .FirstOrDefault(country =>
                phoneValue.StartsWith(NormalizeDialCode(country.DialCode), StringComparison.Ordinal));
    }

    private static string GetLocalPhoneValue(string phoneValue, CountryInfo country)
    {
        if (string.IsNullOrEmpty(phoneValue))
            return string.Empty;

        if (country == null)
            return phoneValue;

        var normalizedDialCode = NormalizeDialCode(country.DialCode);
        return phoneValue.StartsWith(normalizedDialCode, StringComparison.Ordinal)
            ? phoneValue[normalizedDialCode.Length..]
            : phoneValue;
    }

    private static string NormalizeDialCode(string dialCode)
        => dialCode.Replace(" ", string.Empty);

    private static HtmlBuilder GetOptionContent(CountryInfo countryInfo)
    {
        var content = new HtmlBuilder();
        content.Append(HtmlTag.Span, span => span.WithCssClass($"fi fi-{countryInfo.Code.ToLower()}"));
        content.Append(HtmlTag.Span, span => span.AppendText($"\u00A0{countryInfo.DialCode}"));

        return content;
    }
}
