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

public class FieldValidationService(ExpressionsService expressionsService, IStringLocalizer<MasterDataResources> localizer)
{
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private IStringLocalizer<MasterDataResources> Localizer { get; } = localizer;

    public IDictionary<string, string> ValidateFields(
        FormElement formElement, 
        IDictionary<string, object> formValues, 
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
            var isVisible = ExpressionsService.GetBoolValue(field.VisibleExpression, formState);
            if (!isVisible)
                continue;

            var isEnabled = ExpressionsService.GetBoolValue(field.EnableExpression, formState);
            if (!isEnabled)
                continue;

            object value;
            if (formValues.ContainsKey(field.Name) && formValues[field.Name] != null)
                value = formValues[field.Name];
            else
                value = "";

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
                error = Localizer["{0} field is required", fieldName];
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
                {
                    return Localizer["{0} field invalid email", fieldName];
                }

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
                    return Localizer["{0} field has an invalid time", fieldName];
                }

                break;
            case FormComponent.Cnpj:
                if (!Validate.ValidCnpj(valueString))
                {
                    return Localizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.Cpf:
                if (!Validate.ValidCpf(valueString))
                {
                    return Localizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.CnpjCpf:
                if (!Validate.ValidCpfCnpj(valueString))
                {
                    return Localizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.Tel:
                if (!Validate.ValidTel(valueString))
                {
                    return Localizer["{0} field invalid phone", fieldName];
                }

                break;
            case FormComponent.Number:
            case FormComponent.Slider:
                if (field.Attributes.TryGetValue(FormElementField.MinValueAttribute, out var minValue))
                {
                    if (double.Parse(value?.ToString()) < (double?)minValue)
                        return Localizer["{0} field needs to be greater than {1}", fieldName, minValue];
                }

                if (field.Attributes.TryGetValue(FormElementField.MaxValueAttribute, out var maxValue))
                {
                    if (double.Parse(value?.ToString()) > (double?)maxValue)
                        return Localizer["{0} field needs to be less or equal than {1}", fieldName, maxValue];
                }

                break;
            case FormComponent.Text:
            case FormComponent.TextArea:
                if (field.ValidateRequest && (value?.ToString().ToLower().Contains("<script") ?? false))
                {
                    return Localizer["{0} field contains invalid or not allowed character", fieldName];
                }

                break;
        }

        return null;
    }

    private string ValidateDataType(ElementField field, object value, string fieldName)
    {
        switch (field.DataType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
            case FieldType.DateTime2:
                if (!DateTime.TryParse(value?.ToString(), out var date) || date.Year < 1900)
                {
                    return Localizer["{0} field is a invalid date",
                        fieldName];
                }

                break;
            case FieldType.Int:
                if (!int.TryParse(value?.ToString(), out _))
                {
                    return Localizer["{0} field has an invalid number",
                        fieldName];
                }

                break;
            case FieldType.Float:
                if (!double.TryParse(value?.ToString(), out _))
                {
                    return Localizer["{0} field has an invalid number",
                        fieldName];
                }

                break;
            default:
                if (value is not bool && value?.ToString().Length > field.Size && field.Size > 0)
                {
                    return Localizer["{0} field cannot contain more than {1} characters",
                        fieldName, field.Size];
                }

                break;
        }

        return null;
    }

    private static string GetFieldLinkHtml(string fieldName, string label)
    {
        var link = new HtmlBuilder(HtmlTag.A);
        link.WithAttribute("href", "#void");
        link.WithAttribute("onclick", $"javascript:$('#{fieldName}').focus();");
        link.WithCssClass("alert-link");
        link.AppendText(label ?? fieldName);

        return link.ToString();
    }
}