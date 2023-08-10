using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController : DataDictionaryController
{
    private DbLoggerOptions Options { get; }
    private Element LoggerElement { get;  }
    
    private IEntityRepository EntityRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public LogController(IOptions<DbLoggerOptions> options, IEntityRepository entityRepository,IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        EntityRepository = entityRepository;
        StringLocalizer = stringLocalizer;
        Options = options.Value;
        LoggerElement = DbLoggerElement.GetInstance(Options);
    }

    public ActionResult Index()
    {
        if (!EntityRepository.TableExists(Options.TableName))
        {
            EntityRepository.CreateDataModel(LoggerElement);
        }
        
        return View(nameof(Index),Options.TableName);
    }

    [HttpGet]
    public async Task<IActionResult> ClearAll()
    {
        string sql = $"TRUNCATE TABLE {Options.TableName}";
        
        await EntityRepository.SetCommandAsync(sql);
        
        return RedirectToAction("Index");
    }
}