using System;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class FormElementRulesService(
    IValidationDictionary validationDictionary,
    IDataDictionaryRepository dataDictionaryRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryRepository, stringLocalizer)
{
    public async Task SaveAsync(string elementName, FormElementRule rule)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);

        if (!Validate(rule, formElement))
            return;

        if (rule.Id == 0)
        {
            rule.Id = formElement.Rules.Count == 0
                ? 1
                : formElement.Rules.Max(v => v.Id) + 1;

            formElement.Rules.Add(rule);
        }
        else
        {
            for (var i = 0; i < formElement.Rules.Count; i++)
            {
                if (formElement.Rules[i].Id != rule.Id)
                    continue;

                formElement.Rules[i] = rule;
                break;
            }
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public async Task DeleteAsync(string elementName, int ruleId)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var rule = formElement.Rules.FirstOrDefault(v => v.Id == ruleId);
        if (rule == null)
            return;

        formElement.Rules.Remove(rule);
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public bool Validate(FormElementRule rule, FormElement formElement)
    {
        ValidateScriptName(rule.Name);

        if (!rule.RunOnInsert && !rule.RunOnUpdate && !rule.RunOnDelete)
            AddError(nameof(rule.RunOnInsert), StringLocalizer["Select at least one operation."]);

        if (string.IsNullOrWhiteSpace(rule.Script))
            AddError(nameof(rule.Script), StringLocalizer["Required [Script] field"]);

        if (formElement.Rules.Any(v =>
                v.Id != rule.Id &&
                v.Name.Equals(rule.Name, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(rule.Name), StringLocalizer["There is already a rule with this name."]);
        }

        return IsValid;
    }
}
