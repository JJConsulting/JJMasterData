using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LocalizationController : DataDictionaryController
{
    private readonly string? _localizationTableName;
    public LocalizationController(IOptions<JJMasterDataCommonsOptions> coreOptions)
    {
        _localizationTableName = coreOptions.Value.LocalizationTableName;
    }

    public ActionResult Index()
    {
        
        if (string.IsNullOrEmpty(_localizationTableName))
        {
            throw new JJMasterDataException("Resources table not found.");
        }
        
        return View(nameof(Index),_localizationTableName);
    }

}