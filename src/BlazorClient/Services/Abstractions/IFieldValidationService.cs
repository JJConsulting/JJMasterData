using System.ComponentModel.DataAnnotations;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services.Abstractions;

public interface IFieldValidationService
{
    IEnumerable<ValidationResult> ValidateFields(FormElementList fields,IDictionary<string,dynamic> values, bool enableErrorLink = true);
    string? ValidateField(FormElementField field, string? value, bool enableErrorLink);
}