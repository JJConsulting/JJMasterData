#nullable disable warnings
using System;
using System.Globalization;
using System.Linq;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.UI.Components.Phone;

public sealed class JJPhoneGroup(IHttpContextAccessor formValues) : JJTextBox(formValues)
{
    public string GroupCssClass { get; set; }
    public string Id { get; set; }

    public override HtmlBuilder GetHtmlBuilder()
    {
        Attributes.TryGetValue(FormElementField.DefaultFormatAttribute, out var defaultCountry);

        var input = base.GetHtmlBuilder();
        
        var selectedCountry = GetCountryFromPhoneValue(Text);

        if (selectedCountry == null)
        {
            if (!string.IsNullOrEmpty(defaultCountry) && CountryHelper.TryGet(defaultCountry, out var configuredCountry))
            {
                selectedCountry = configuredCountry;
            }
            else
            {
                selectedCountry = GetCountryFromCulture(CultureInfo.CurrentUICulture);
            }
        }
        
        var phoneValue = Text ?? string.Empty;
        var localPhoneValue = GetLocalPhoneValue(phoneValue, selectedCountry);
        var phoneGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("jj-phone-group");

        var hiddenInput = new HtmlBuilder(HtmlTag.Input)
            .WithAttribute("type", "hidden")
            .WithName(Name)
            .WithId($"{Id ?? Name}_hidden")
            .WithValue(phoneValue)
            .WithCssClass("jj-phone-hidden-input");

        var dropDownGroup = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("input-group jjform-action ")
            .WithCssClass(GroupCssClass)
            .AppendSelect(select =>
            {
                select.WithCssClass("form-select tom-select w-auto jj-phone-select");
                select.WithCssClassIf(!Enabled || ReadOnly, "disabled");
                select.WithAttributeIf(!Enabled || ReadOnly, "disabled");
                foreach (var countryInfo in CountryHelper.All.OrderBy(c => c.DialCode))
                {
                    select.AppendOption(opt =>
                    {
                        opt.WithAttributeIf(selectedCountry?.Id == countryInfo.Id, "selected", true.ToString());
                        opt.WithAttribute("dial-code", countryInfo.DialCode);
                        opt.WithValue(countryInfo.CountryCode);
                        opt.WithAttribute("data-content", GetOptionContent(countryInfo).ToString());
                        opt.AppendText($"{countryInfo.Name} {countryInfo.DialCode}");
                    });
                }
            });

        input.WithCssClass("jj-phone-input");
        input.WithAttribute("id", Id ?? Name);
        input.WithAttribute("name", $"{Name}_display");
        input.WithAttribute("value", localPhoneValue);
        dropDownGroup.Append(input);

        phoneGroup.Append(hiddenInput);
        phoneGroup.Append(dropDownGroup);

        return phoneGroup;
    }

    private static CountryInfo? GetCountryFromPhoneValue(string phoneValue)
    {
        if (string.IsNullOrWhiteSpace(phoneValue) || !phoneValue.StartsWith('+'))
            return null;

        return CountryHelper.All
         
            .FirstOrDefault(country =>
                phoneValue.StartsWith(NormalizeDialCode(country.DialCode), StringComparison.Ordinal));
    }

    private static string? GetLocalPhoneValue(string phoneValue, CountryInfo? country)
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

    private static CountryInfo? GetCountryFromCulture(CultureInfo culture)
    {
        try
        {
            var regionCode = new RegionInfo(culture.Name).TwoLetterISORegionName;
            return CountryHelper.TryGet(regionCode, out var country) ? country : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static HtmlBuilder GetOptionContent(CountryInfo countryInfo)
    {
        var content = new HtmlBuilder();
        content.Append(HtmlTag.Span, span => span.WithCssClass($"fi fi-{countryInfo.CountryCode.ToLowerInvariant()}"));
        content.Append(HtmlTag.Span, span => span.AppendText($"\u00A0{countryInfo.DialCode}"));

        return content;
    }
}
