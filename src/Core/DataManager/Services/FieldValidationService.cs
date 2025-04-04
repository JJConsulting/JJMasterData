using System;
using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Validations;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class FieldValidationService(
    ExpressionsService expressionsService,
    IStringLocalizer<MasterDataResources> localizer)
{
    public Dictionary<string, string> ValidateFields(
        FormElement formElement,
        Dictionary<string, object> formValues,
        PageState pageState,
        bool enableErrorLink)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (formValues == null)
            throw new ArgumentNullException(nameof(formValues));

        var errors = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        var formState = new FormStateData(formValues, pageState);
        foreach (var field in formElement.Fields)
        {
            var isVisible = expressionsService.GetBoolValue(field.VisibleExpression, formState);
            if (!isVisible)
                continue;

            var isEnabled = expressionsService.GetBoolValue(field.EnableExpression, formState);
            if (!isEnabled)
                continue;


            if (!formValues.TryGetValue(field.Name, out var value) || value == null)
            {
                value = "";
            }

            var error = ValidateField(field, field.Name, value, enableErrorLink);
            if (!string.IsNullOrEmpty(error))
                errors.Add(field.Name, error);
        }

        return errors;
    }

    public string ValidateField(FormElementField field, string fieldId, object value, bool enableErrorLink = true)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        string fieldName = enableErrorLink ? GetFieldLinkHtml(fieldId, field.LabelOrName) : field.Label;

        string error = null;

        if (string.IsNullOrEmpty(value?.ToString()))
        {
            if (field.IsRequired || field.IsPk)
            {
                error = localizer["{0} field is required", fieldName];
            }
        }
        else
        {
            error = ValidateDataType(field, value, fieldName);

            error ??= ValidateComponent(field, value, fieldName);
        }

        return error;
    }

    private string ValidateComponent(FormElementField field, object value, string fieldName)
    {
        var valueString = value.ToString();
        switch (field.Component)
        {
            case FormComponent.Email:
                if (!Validate.ValidEmail(valueString))
                    return localizer["{0} field has an invalid email", fieldName];
                break;
            case FormComponent.Hour:
                var hourFormat = valueString.Length == 5 ? "HH:mm" : "HH:mm:ss";

                var valid = DateTime.TryParseExact(valueString,
                    hourFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _);
                if (!valid)
                {
                    return localizer["{0} field has an invalid time", fieldName];
                }

                break;
            case FormComponent.Cnpj:
                if (!Validate.ValidCnpj(valueString))
                {
                    return localizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.Cpf:
                if (!Validate.ValidCpf(valueString))
                {
                    return localizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.CnpjCpf:
                if (!Validate.ValidCpfCnpj(valueString))
                {
                    return localizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.Tel:
                if (!Validate.ValidTel(valueString))
                {
                    return localizer["{0} field has an invalid phone", fieldName];
                }

                break;
            case FormComponent.Number:
            case FormComponent.Slider:
                if (field.Attributes.TryGetValue(FormElementField.MinValueAttribute, out var minValue))
                {
                    if (double.Parse(value?.ToString()) < (double?)minValue)
                        return localizer["{0} field needs to be greater than {1}", fieldName, minValue];
                }

                if (field.Attributes.TryGetValue(FormElementField.MaxValueAttribute, out var maxValue))
                {
                    if (double.Parse(value?.ToString()) > (double?)maxValue)
                        return localizer["{0} field needs to be less or equal than {1}", fieldName, maxValue];
                }

                break;
            case FormComponent.Text:
            case FormComponent.TextArea:
                if (field.ValidateRequest &&
                    value.ToString()?.IndexOf("<script", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return localizer["{0} field contains invalid or not allowed character", fieldName];
                }

                break;
        }

        return null;
    }

    private string ValidateDataType(FormElementField field, object value, string fieldName)
    {
        var dataType = field.DataType;
        switch (dataType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
            case FieldType.DateTime2:
                if (!DateTime.TryParse(value?.ToString(), out _))
                {
                    return localizer["{0} field is an invalid date",
                        fieldName];
                }
                break;
            case FieldType.Time:
                if (!TimeSpan.TryParse(value?.ToString(), out _))
                {
                    return localizer["{0} field is an invalid time",
                        fieldName];
                }

                break;
            case FieldType.Int:
                if (value is not bool && !int.TryParse(value?.ToString(), NumberStyles.Number,
                        CultureInfo.CurrentCulture, out _))
                {
                    return localizer["{0} field has an invalid number",
                        fieldName];
                }

                break;
            case FieldType.Float:
                if (!double.TryParse(value?.ToString(), out _))
                {
                    return localizer["{0} field has an invalid number",
                        fieldName];
                }

                break;
            case FieldType.Decimal:
                if (!decimal.TryParse(value?.ToString(), out var decimalValue))
                {
                    return localizer["{0} field has an invalid number",
                        fieldName];
                }
                
                var integerLength = (int)Math.Floor(Math.Log10(Math.Abs((double)decimalValue)) + 1);
                var decimalLength = BitConverter.GetBytes(decimal.GetBits(decimalValue)[3])[2];

                if (integerLength + decimalLength > field.Size)
                {
                    return localizer["Field {0} exceeds maximum size of {0} digits.", fieldName, field.Size];
                }
                
                if (decimalLength > field.NumberOfDecimalPlaces)
                {
                    return localizer["Field {0} exceeds maximum number of {1} decimal places.", fieldName, field.NumberOfDecimalPlaces];
                }
                break;
            case FieldType.Varchar:
            case FieldType.NVarchar:
            {
                if (field.Size > 0 && field.Component is FormComponent.Text or FormComponent.TextArea)
                {
                    return localizer["Field {0} cannot contain more than {1} characters.",
                        fieldName, field.Size];
                }
                break;
            }
        }

        return null;
    }

    private static string GetFieldLinkHtml(string fieldName, string label)
    {
        var link = new HtmlBuilder(HtmlTag.A);
        link.WithAttribute("href", "#void");
        link.WithOnClick($"javascript:$('#{fieldName}').focus();");
        link.WithCssClass("alert-link");
        link.AppendText(label ?? fieldName);

        return link.ToString();
    }
}