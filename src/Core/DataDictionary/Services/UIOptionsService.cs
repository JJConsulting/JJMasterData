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

    private async Task<bool> ValidateOptions(FormElementOptions options, string elementName)
    {

        if (options.Grid.EnableMultSelect)
        {
            var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);
            var pks = formElement.Fields.ToList().FindAll(x => x.IsPk);
            if (pks.Count == 0)
            {
                AddError("EnableMultSelect", StringLocalizer["You cannot enable MultiSelect without setting a primary key"]);
            }
        }

        return IsValid;
    }


    public async Task<bool> EditOptionsAsync(FormElementOptions options,string elementName)
    {
        try
        {
            if (await ValidateOptions(options, elementName))
            {
                var dicParser =await DataDictionaryRepository.GetMetadataAsync(elementName);
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