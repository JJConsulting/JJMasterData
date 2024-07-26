using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class UIOptionsService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository, IStringLocalizer<MasterDataResources> stringLocalizer)
    : BaseService(validationDictionary, dataDictionaryRepository,stringLocalizer)
{
    private async Task<bool> ValidateOptions(FormElementOptions options, string elementName)
    {

        if (options.Grid.EnableMultiSelect)
        {
            var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
            var pks = formElement.Fields.FindAll(x => x.IsPk);
            if (pks.Count == 0)
            {
                AddError("EnableMultiSelect", StringLocalizer["You cannot enable MultiSelect without setting a primary key"]);
            }
        }

        if (options.Grid.RecordsPerPage < 5)
        {
            AddError("RecordsPerPage", StringLocalizer["You need at least 5 records per page"]);

        }
        
        return IsValid;
    }


    public async Task<bool> EditOptionsAsync(FormElementOptions options,string elementName)
    {
        try
        {
            if (await ValidateOptions(options, elementName))
            {
                var formElement =await DataDictionaryRepository.GetFormElementAsync(elementName);
                
                formElement.Options.Form = options.Form;
                formElement.Options.Grid = options.Grid;
                formElement.Options.EnableAuditLog = options.EnableAuditLog;
                formElement.Options.UseFloatingLabels = options.UseFloatingLabels;
                
                await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
            }
        }
        catch (Exception ex)
        {
            AddError("Options", ex.Message);
        }
         
        return IsValid;
    }
}