using System;
using System.Linq;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Phone;

public sealed class JJPhoneGroup(IFormValues formValues) : JJTextBox(formValues)
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
        var dropDownGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass)
            .AppendSelect(select =>
            {
                select.WithCssClass("selectpicker w-auto jj-phone-select");
                select.WithAttribute("data-live-search", "true");
                select.WithAttribute("data-style-base", "form-select form-dropdown");
                foreach (var countryInfo in CountryHelper.All)
                {
                    select.AppendOption(opt =>
                    {
                        opt.WithAttributeIf(selectedCountry?.Code == countryInfo.Code, "selected", true.ToString());
                        opt.WithAttribute("dial-code", countryInfo.DialCode);
                        opt.WithValue(countryInfo.Code);
                        opt.WithAttribute("data-content", GetOptionContent(countryInfo).ToString());
                    });
                }
            });

        dropDownGroup.Append(HtmlTag.Input, hidden =>
        {
            hidden.WithAttribute("type", "hidden");
            hidden.WithName(Name);
            hidden.WithId($"{Name}_hidden");
            hidden.WithValue(phoneValue);
            hidden.WithCssClass("jj-phone-hidden-input");
        });

        input.WithCssClass("jj-phone-input");
        input.WithAttribute("id", Name);
        input.WithAttribute("name", $"{Name}_display");
        input.WithAttribute("value", localPhoneValue);
        dropDownGroup.Append(input);

        return dropDownGroup;
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
