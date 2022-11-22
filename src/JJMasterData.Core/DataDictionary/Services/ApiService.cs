using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ApiService : BaseService
{
    public ApiService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public bool EditApi(Dictionary dicParser)
    {
        if (ValidateApi(dicParser))
            DicDao.SetDictionary(dicParser);

        return IsValid;
    }

    public bool ValidateApi(Dictionary dicParser)
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