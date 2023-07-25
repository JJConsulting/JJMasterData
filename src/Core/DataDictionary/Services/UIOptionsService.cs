using System;
using System.Linq;
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

    private bool ValidateOptions(FormElementOptions options, string dictionaryName)
    {

        if (options.Grid.EnableMultSelect)
        {
            var formElement = DataDictionaryRepository.GetMetadata(dictionaryName);
            var pks = formElement.Fields.ToList().FindAll(x => x.IsPk);
            if (pks.Count == 0)
            {
                AddError("EnableMultSelect", StringLocalizer["You cannot enable MultiSelect without setting a primary key"]);
            }
        }

        return IsValid;
    }


    public bool EditOptions(FormElementOptions options,string dictionaryName)
    {
        try
        {
            if (ValidateOptions(options, dictionaryName))
            {
                var dicParser = DataDictionaryRepository.GetMetadata(dictionaryName);
                dicParser.Options.Form = options.Form;
                dicParser.Options.Grid = options.Grid;

                DataDictionaryRepository.InsertOrReplace(dicParser);
            }
        }
        catch (Exception ex)
        {
            AddError("Options", ex.Message);
        }
         
        return IsValid;
    }
}