using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ApiService : BaseService
{
    public ApiService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository)
        : base(validationDictionary, dictionaryRepository)
    {
    }

    public bool EditApi(DataDictionary dicParser)
    {
        if (ValidateApi(dicParser))
            DictionaryRepository.SetDictionary(dicParser);

        return IsValid;
    }

    public bool ValidateApi(DataDictionary dicParser)
    {
        bool hasApiGetEnabled;

        if (dicParser.Api.EnableGetDetail & dicParser.Api.EnableGetAll)
            hasApiGetEnabled = true;
        else
            hasApiGetEnabled = false;

        if (dicParser.Table.Sync & !hasApiGetEnabled)
        {
            AddError("Api", Translate.Key("To enable sync the get APIs must be enabled."));
        }

        return IsValid;
    }

}