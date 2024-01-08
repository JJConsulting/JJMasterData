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

public class LogController(IFormElementComponentFactory<JJFormView> formViewFactory,
        LoggerFormElementFactory loggerFormElementFactory,
        IEntityRepository entityRepository,
        IOptions<DbLoggerOptions> options)
    : DataDictionaryController
{
    private DbLoggerOptions Options { get; } = options.Value;

    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private LoggerFormElementFactory LoggerFormElementFactory { get; } = loggerFormElementFactory;
    private IEntityRepository EntityRepository { get; } = entityRepository;

    public async Task<IActionResult> Index()
    {
        var formElement = LoggerFormElementFactory.GetFormElement();

        if (!await EntityRepository.TableExistsAsync(Options.TableName))
        {
            await EntityRepository.CreateDataModelAsync(formElement);
        }

        var formView = FormViewFactory.Create(formElement);
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
            args.HtmlResult = new HtmlBuilder(message ?? string.Empty);

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

        await EntityRepository.SetCommandAsync(new DataAccessCommand(sql));

        return RedirectToAction("Index");
    }
}