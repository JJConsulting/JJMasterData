using System;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class UIOptionsService : BaseService
{
    public UIOptionsService(IValidationDictionary validationDictionary, IDataDictionaryRepository dataDictionaryRepository, IStringLocalizer<JJMasterDataResources> stringLocalizer)
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {
    }

    private async Task<bool> ValidateOptions(FormElementOptions options, string dictionaryName)
    {

        if (options.Grid.EnableMultSelect)
        {
            var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
            var pks = formElement.Fields.ToList().FindAll(x => x.IsPk);
            if (pks.Count == 0)
            {
                AddError("EnableMultSelect", StringLocalizer["You cannot enable MultiSelect without setting a primary key"]);
            }
        }

        return IsValid;
    }


    public async Task<bool> EditOptionsAsync(FormElementOptions options,string dictionaryName)
    {
        try
        {
            if (await ValidateOptions(options, dictionaryName))
            {
                var dicParser =await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
                dicParser.Options.Form = options.Form;
                dicParser.Options.Grid = options.Grid;

                await DataDictionaryRepository.InsertOrReplaceAsync(dicParser);
            }
        }
        catch (Exception ex)
        {
            AddError("Options", ex.Message);
        }
         
        return IsValid;
    }
}