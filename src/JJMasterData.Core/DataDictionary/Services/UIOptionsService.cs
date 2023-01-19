using System;
using System.Linq;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class UIOptionsService : BaseService
{
    public UIOptionsService(IValidationDictionary validationDictionary, IDataDictionaryRepository dataDictionaryRepository)
        : base(validationDictionary, dataDictionaryRepository)
    {
    }

    private bool ValidateOptions(MetadataOptions uIMetadataOptions, string dictionaryName)
    {

        if (uIMetadataOptions.Grid.EnableMultSelect)
        {
            var dicParser = DataDictionaryRepository.GetMetadata(dictionaryName);
            var pks = dicParser.Table.Fields.ToList().FindAll(x => x.IsPk);
            if (pks.Count == 0)
            {
                AddError("EnableMultSelect", Translate.Key("You cannot enable MultiSelect without setting a primary key"));
            }
        }

        return IsValid;
    }


    public bool EditOptions(MetadataOptions uIMetadataOptions,string dictionaryName)
    {
        try
        {
            if (ValidateOptions(uIMetadataOptions, dictionaryName))
            {
                var dicParser = DataDictionaryRepository.GetMetadata(dictionaryName);
                dicParser.Options.Form = uIMetadataOptions.Form;
                dicParser.Options.Grid = uIMetadataOptions.Grid;

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