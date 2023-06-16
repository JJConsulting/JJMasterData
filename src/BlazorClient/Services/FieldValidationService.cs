using System.ComponentModel.DataAnnotations;
using System.Globalization;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services;


public class FieldValidationService : IFieldValidationService
{
    public IEnumerable<ValidationResult> ValidateFields(FormElementList fields,IDictionary<string,dynamic> values, bool enableErrorLink = true)
    {
        foreach (var field in fields)
        {
            if (values.TryGetValue(field.Name, out var value))
            {
                string? result = ValidateField(field, value, enableErrorLink);

                if (result != null)
                {
                    yield return new ValidationResult(result, new []{field.Name});
                }
            }
        }
    }
    
    public string? ValidateField(FormElementField field, string value, bool enableErrorLink = true)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));
        
        string fieldName = enableErrorLink ? GetFieldLinkHtml(field.Name, field.Label) : field.Label;

        string? error = null;
        
        if (string.IsNullOrEmpty(value))
        {
            if(field.IsRequired || field.IsPk)
            {
                error = Translate.Key("{0} field is required", fieldName);
            }
        }
        else
        {
            error = ValidateDataType(field, value, fieldName);
        
            error ??= ValidateComponent(field, value, fieldName);
        }
        
        return error;
    }

    // Helper method to retrieve attribute value or default
    private T? GetAttributeValueOrDefault<T>(IDictionary<string, dynamic> attributes, string key)
    {
        if (attributes.TryGetValue(key, out dynamic? value) && value is T)
            return (T)value;

        return default;
    }
    
    private string? ValidateComponent(FormElementField field, string value,  string fieldName)
    {
        switch (field.Component)
        {
            case FormComponent.Email:
                if (!Validate.ValidEmail(value))
                {
                    return Translate.Key("{0} field invalid email", fieldName);
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
                    return Translate.Key("{0} field has an invalid time", fieldName);
                }


                break;
            case FormComponent.Cnpj:
                if (!Validate.ValidCnpj(value))
                {
                    return Translate.Key("{0} field has an invalid value", fieldName);
                }

                break;
            case FormComponent.Cpf:
                if (!Validate.ValidCpf(value))
                {
                    return Translate.Key("{0} field has an invalid value", fieldName);
                }

                break;
            case FormComponent.CnpjCpf:
                if (!Validate.ValidCpfCnpj(value))
                {
                    return Translate.Key("{0} field has an invalid value", fieldName);
                }

                break;
            case FormComponent.Tel:
                if (!Validate.ValidTel(value))
                {
                    return Translate.Key("{0} field invalid phone", fieldName);
                }

                break;
            case FormComponent.Number:
            case FormComponent.Slider:
                if (field.DataType == FieldType.Int)
                {
                    if (int.Parse(value) < GetAttributeValueOrDefault<float>(field.Attributes, FormElementField.MinValueAttribute))
                        return Translate.Key("{0} field needs to be greater than {1}", fieldName, GetAttributeValueOrDefault<float>(field.Attributes, FormElementField.MinValueAttribute));

                    if (int.Parse(value) > GetAttributeValueOrDefault<float>(field.Attributes, FormElementField.MaxValueAttribute))
                        return Translate.Key("{0} field needs to be less or equal than {1}", fieldName, GetAttributeValueOrDefault<float>(field.Attributes, FormElementField.MaxValueAttribute));
                }
                else
                {
                    if (float.Parse(value, CultureInfo.CurrentCulture) < GetAttributeValueOrDefault<float>(field.Attributes, FormElementField.MinValueAttribute))
                        return Translate.Key("{0} field needs to be greater than {1}", fieldName, GetAttributeValueOrDefault<float>(field.Attributes, "MinValue"));

                    if (float.Parse(value, CultureInfo.CurrentCulture) > GetAttributeValueOrDefault<float>(field.Attributes, FormElementField.MaxValueAttribute))
                        return Translate.Key("{0} field needs to be less or equal than {1}", fieldName, GetAttributeValueOrDefault<float>(field.Attributes, "MaxValue"));
                }
               
                break;
            case FormComponent.Text:
            case FormComponent.TextArea:
                if (field.ValidateRequest && value.ToLower().Contains("<script"))
                {
                    return Translate.Key("{0} field contains invalid or not allowed character", fieldName);
                }

                break;
        }

        return null;
    }

    private static string? ValidateDataType(ElementField field, string value, string fieldName)
    {
        switch (field.DataType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
            case FieldType.DateTime2:
                if (!DateTime.TryParse(value, out var date) || date.Year < 1900)
                {
                    return Translate.Key("{0} field is a invalid date",
                        fieldName);
                }

                break;
            case FieldType.Int:
                if (!int.TryParse(value, out _))
                {
                    return Translate.Key("{0} field has an invalid number",
                        fieldName);
                }

                break;
            case FieldType.Float:
                if (!double.TryParse(value, out _))
                {
                    return Translate.Key("{0} field has an invalid number",
                        fieldName);
                }

                break;
            default:
                if (value.Length > field.Size && field.Size > 0)
                {
                    return Translate.Key("{0} field cannot contain more than {1} characters",
                        fieldName, field.Size);
                }

                break;
        }

        return null;
    }

    private static string GetFieldLinkHtml(string fieldName, string? label)
    {
        return $"<a onclick=\"javascript:$('#{fieldName}').focus();\" class=\"alert-link\">{label ?? fieldName}</a>";
    }
}