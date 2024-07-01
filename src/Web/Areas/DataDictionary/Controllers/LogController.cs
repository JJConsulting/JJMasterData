using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController(IFormElementComponentFactory<JJFormView> formViewFactory,
        LoggerFormElementFactory loggerFormElementFactory,
        IEntityRepository entityRepository,
        IOptionsSnapshot<DbLoggerOptions> options)
    : DataDictionaryController
{
    private DbLoggerOptions Options { get; } = options.Value;

    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private LoggerFormElementFactory LoggerFormElementFactory { get; } = loggerFormElementFactory;
    private IEntityRepository EntityRepository { get; } = entityRepository;

    public async Task<IActionResult> Index(bool isModal)
    {
        var formElement = LoggerFormElementFactory.GetFormElement(isModal);

        if (!await EntityRepository.TableExistsAsync(Options.TableName))
        {
            await EntityRepository.CreateDataModelAsync(formElement,[]);
        }

        var formView = FormViewFactory.Create(formElement);
        formView.ShowTitle = !isModal;
        
        if (!formView.GridView.CurrentOrder.Any())
        {
            formView.GridView.CurrentOrder.AddOrReplace(Options.CreatedColumnName, OrderByDirection.Desc);
        }

        formView.GridView.OnRenderCellAsync += (_, args) =>
        {
            if (!args.Field.Name.Equals(Options.MessageColumnName))
                return ValueTask.CompletedTask;
            
            var message = args.DataRow[Options.MessageColumnName].ToString()?.Replace("\n", "<br>");
            args.HtmlResult = new HtmlBuilder(message ?? string.Empty);

            return ValueTask.CompletedTask;
        };
        
        var result = await formView.GetResultAsync();

        if (result is IActionResult actionResult)
            return actionResult;

        return View(new LogViewModel
        {
            FormViewHtml = result.Content,
            IsModal = isModal
        });
    }

    [HttpGet]
    public async Task<IActionResult> ClearAll(bool isModal)
    {
        var sql = $"TRUNCATE TABLE {Options.TableName}";

        await EntityRepository.SetCommandAsync(new DataAccessCommand(sql));

        return RedirectToAction("Index", new {isModal});
    }
}