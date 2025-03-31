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
    private readonly DbLoggerOptions _options = options.Value;
    public async Task<IActionResult> Index(bool isModal)
    {
        var formElement = loggerFormElementFactory.GetFormElement(isModal);

        if (!await entityRepository.TableExistsAsync(_options.TableName))
        {
            await entityRepository.CreateDataModelAsync(formElement,[]);
        }

        var formView = formViewFactory.Create(formElement);
        formView.ShowTitle = !isModal;
        
        if (!formView.GridView.CurrentOrder.Any())
        {
            formView.GridView.CurrentOrder.AddOrReplace(_options.CreatedColumnName, OrderByDirection.Desc);
        }

        formView.GridView.OnRenderCellAsync += (_, args) =>
        {
            if (!args.Field.Name.Equals(_options.MessageColumnName))
                return ValueTask.CompletedTask;
            
            var message = args.DataRow[_options.MessageColumnName]?.ToString()?.Replace("\n", "<br>");
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
        var schema = string.IsNullOrEmpty(_options.TableSchema) ? "dbo" : _options.TableSchema;
        var sql = $"TRUNCATE TABLE {schema}.{_options.TableName}";

        await entityRepository.SetCommandAsync(new DataAccessCommand(sql));

        return RedirectToAction("Index", new {isModal});
    }
}