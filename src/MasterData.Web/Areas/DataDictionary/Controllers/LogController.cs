using System.Web;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using JJMasterData.Web.Areas.DataDictionary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController(
        IFormElementComponentFactory<JJFormView> formViewFactory,
        LoggerFormElementFactory loggerFormElementFactory,
        RelativeDateFormatter formatter,
        IEntityRepository entityRepository,
        IOptionsSnapshot<DbLoggerOptions> options)
    : DataDictionaryController
{
    private readonly DbLoggerOptions _options = options.Value;
    public async Task<IActionResult> Index([FromQuery] bool isModal)
    {
        var formElement = loggerFormElementFactory.GetFormElement(isModal);

        var formView = formViewFactory.Create(formElement);
        formView.ShowTitle = !isModal;
        
        if (!formView.GridView.CurrentOrder.Any())
        {
            formView.GridView.CurrentOrder.AddOrReplace(_options.CreatedColumnName, OrderByDirection.Desc);
        }

        formView.GridView.OnRenderCellAsync += (_, args) =>
        {
            if (args.Field.Name == _options.CreatedColumnName)
            {
                args.HtmlResult = new HtmlBuilder().AppendSpan(span =>
                {
                    var createdAt = (DateTime)args.DataRow[_options.CreatedColumnName]!;
                    span.WithToolTip(formatter.ToRelativeString(createdAt));
                    span.AppendText(createdAt.ToString("dd/MM/yyyy HH:mm:ss"));
                });
            }
            else if (args.Field.Name == _options.MessageColumnName)
            {
                args.HtmlResult = new HtmlBuilder().AppendDiv(div =>
                {
                    div.WithCssClass("fw-bold");
                    div.AppendText(args.DataRow[_options.CategoryColumnName]?.ToString() ?? "");
                }).AppendDiv(div =>
                {
                    var message = HttpUtility.HtmlEncode(args.DataRow[_options.MessageColumnName]?.ToString() ?? "")
                        .Replace("\n", "<br>");
                
                    div.AppendText(message);
                    div.WithCssClass("font-monospace");
                });
            }

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