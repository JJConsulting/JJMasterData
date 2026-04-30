using System;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class FormElementValidationsService(
    IValidationDictionary validationDictionary,
    IDataDictionaryRepository dataDictionaryRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryRepository, stringLocalizer)
{
    public async Task SaveAsync(string elementName, FormElementValidation validation)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);

        if (!Validate(validation, formElement))
            return;

        if (validation.Id == 0)
        {
            validation.Id = formElement.Validations.Count == 0
                ? 1
                : formElement.Validations.Max(v => v.Id) + 1;

            formElement.Validations.Add(validation);
        }
        else
        {
            for (var i = 0; i < formElement.Validations.Count; i++)
            {
                if (formElement.Validations[i].Id != validation.Id)
                    continue;

                formElement.Validations[i] = validation;
                break;
            }
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public async Task DeleteAsync(string elementName, int validationId)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var validation = formElement.Validations.FirstOrDefault(v => v.Id == validationId);
        if (validation == null)
            return;

        formElement.Validations.Remove(validation);
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public bool Validate(FormElementValidation validation, FormElement formElement)
    {
        ValidateScriptName(validation.Name);

        if (string.IsNullOrWhiteSpace(validation.Script))
            AddError(nameof(validation.Script), StringLocalizer["Required [Script] field"]);

        if (formElement.Validations.Any(v =>
                v.Id != validation.Id &&
                v.Name.Equals(validation.Name, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(validation.Name), StringLocalizer["There is already a validation with this name."]);
        }

        return IsValid;
    }
}
