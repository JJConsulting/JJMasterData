using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController(
    IFormElementComponentFactory<JJFormView> formViewFactory,
        LoggerFormElementFactory loggerFormElementFactory,
        IEntityRepository entityRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IOptionsSnapshot<DbLoggerOptions> options)
    : DataDictionaryController
{
    private DbLoggerOptions Options { get; } = options.Value;
    
    public async Task<IActionResult> Index()
    {
        var formElement = loggerFormElementFactory.GetFormElement();

        if (!await entityRepository.TableExistsAsync(Options.TableName))
        {
            await entityRepository.CreateDataModelAsync(formElement,[]);
        }

        var formView = formViewFactory.Create(formElement);
        formView.ShowTitle = false;
        if (!formView.GridView.CurrentOrder.Any())
        {
            formView.GridView.CurrentOrder.AddOrReplace(Options.CreatedColumnName, OrderByDirection.Desc);
        }

        formView.GridView.OnRenderCellAsync += (sender, args) =>
        {
            if (!args.Field.Name.Equals(Options.MessageColumnName))
                return Task.CompletedTask;
            
            var message = args.DataRow[Options.MessageColumnName].ToString()?.Replace("\n", "<br>");

            var localizedMessage = stringLocalizer[message!];
            
            args.HtmlResult = new HtmlBuilder(localizedMessage );

            return Task.CompletedTask;
        };
        
        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;

        return View(nameof(Index), result.Content);
    }

    [HttpGet]
    public async Task<IActionResult> ClearAll()
    {
        var sql = $"TRUNCATE TABLE {Options.TableName}";

        await entityRepository.SetCommandAsync(new DataAccessCommand(sql));

        return RedirectToAction("Index");
    }
}