using System;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class UIOptionsService : BaseService
{
    public UIOptionsService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository)
        : base(validationDictionary, dictionaryRepository)
    {
    }

    private bool ValidateOptions(UIOptions uIOptions, string dictionaryName)
    {
        List<string> listError = new List<string>();

        if (uIOptions.Grid.EnableMultSelect)
        {
            var dicParser = DictionaryRepository.GetDictionary(dictionaryName);
            var pks = dicParser.Table.Fields.ToList().FindAll(x => x.IsPk);
            if (pks.Count == 0)
            {
                AddError("EnableMultSelect", Translate.Key("You cannot enable MultiSelect without setting a primary key"));
            }
        }

        return IsValid;
    }


    public bool EditOptions(UIOptions uIOptions,string dictionaryName)
    {
        try
        {
            if (ValidateOptions(uIOptions, dictionaryName))
            {
                var dicParser = DictionaryRepository.GetDictionary(dictionaryName);
                dicParser.UIOptions.Form = uIOptions.Form;
                dicParser.UIOptions.Grid = uIOptions.Grid;

                DictionaryRepository.SetDictionary(dicParser);
            }
        }
        catch (Exception ex)
        {
            AddError("Options", ex.Message);
        }
         
        return IsValid;
    }
}