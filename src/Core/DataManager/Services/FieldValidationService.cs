using System;
using System.Globalization;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager;

public class FieldValidationService : IFieldValidationService
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public FieldValidationService(IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
    }
    public string ValidateField(FormElementField field, string objname, string value, bool enableErrorLink = true)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        string fieldName = enableErrorLink ? GetFieldLinkHtml(objname, field.Label) : field.Label;

        string error = null;

        if (string.IsNullOrEmpty(value))
        {
            if (field.IsRequired || field.IsPk)
            {
                error = StringLocalizer["{0} field is required", fieldName];
            }
        }
        else
        {
            error = ValidateDataType(field, value, fieldName);

            error ??= ValidateComponent(field, value, fieldName);
        }

        return error;
    }

    private string ValidateComponent(FormElementField field, string value, string fieldName)
    {
        switch (field.Component)
        {
            case FormComponent.Email:
                if (!Validate.ValidEmail(value))
                {
                    return StringLocalizer["{0} field invalid email", fieldName];
                }

                break;
            case FormComponent.Hour:

                var hourFormat = value.Length == 5 ? "HH:mm" : "HH:mm:ss";

                var valid = DateTime.TryParseExact(value,
                    hourFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _);
                if (!valid)
                {
                    return StringLocalizer["{0} field has an invalid time", fieldName];
                }


                break;
            case FormComponent.Cnpj:
                if (!Validate.ValidCnpj(value))
                {
                    return StringLocalizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.Cpf:
                if (!Validate.ValidCpf(value))
                {
                    return StringLocalizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.CnpjCpf:
                if (!Validate.ValidCpfCnpj(value))
                {
                    return StringLocalizer["{0} field has an invalid value", fieldName];
                }

                break;
            case FormComponent.Tel:
                if (!Validate.ValidTel(value))
                {
                    return StringLocalizer["{0} field invalid phone", fieldName];
                }

                break;
            case FormComponent.Number:
            case FormComponent.Slider:
                if (field.Attributes.TryGetValue(FormElementField.MinValueAttribute, out var minValue))
                {
                    if (double.Parse(value) < (double?)minValue)
                        return StringLocalizer["{0} field needs to be greater than {1}", fieldName, minValue];
                }

                if (field.Attributes.TryGetValue(FormElementField.MaxValueAttribute, out var maxValue))
                {
                    if (double.Parse(value) > (double?)maxValue)
                        return StringLocalizer["{0} field needs to be less or equal than {1}", fieldName, maxValue];
                }

                break;
            case FormComponent.Text:
            case FormComponent.TextArea:
                if (field.ValidateRequest && value.ToLower().Contains("<script"))
                {
                    return StringLocalizer["{0} field contains invalid or not allowed character", fieldName];
                }

                break;
        }

        return null;
    }

    private string ValidateDataType(FormElementField field, string value, string fieldName)
    {
        switch (field.DataType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
            case FieldType.DateTime2:
                if (!DateTime.TryParse(value, out var date) || date.Year < 1900)
                {
                    return StringLocalizer["{0} field is a invalid date",
                        fieldName];
                }

                break;
            case FieldType.Int:
                if (!int.TryParse(value, out _))
                {
                    return StringLocalizer["{0} field has an invalid number",
                        fieldName];
                }

                break;
            case FieldType.Float:
                if (!double.TryParse(value, out _))
                {
                    return StringLocalizer["{0} field has an invalid number",
                        fieldName];
                }

                break;
            default:
                if (value.Length > field.Size && field.Size > 0)
                {
                    return StringLocalizer["{0} field cannot contain more than {1} characters",
                        fieldName, field.Size];
                }

                break;
        }

        return null;
    }

    private string GetFieldLinkHtml(string fieldName, string label)
    {
        return $"<a href=\"#void\" onclick=\"javascript:$('#{fieldName}').focus();\" class=\"alert-link\">{label ?? fieldName}</a>";
    }
}