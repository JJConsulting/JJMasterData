using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController(IFormElementComponentFactory<JJFormView> formViewFactory,
        LoggerFormElementFactory loggerFormElementFactory,
        IEntityRepository entityRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IOptions<DbLoggerOptions> options)
    : DataDictionaryController
{
    private DbLoggerOptions Options { get; } = options.Value;

    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; } = formViewFactory;
    private LoggerFormElementFactory LoggerFormElementFactory { get; } = loggerFormElementFactory;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public async Task<IActionResult> Index()
    {
        var formElement = LoggerFormElementFactory.GetFormElement();

        if (!await EntityRepository.TableExistsAsync(Options.TableName))
        {
            await EntityRepository.CreateDataModelAsync(formElement);
        }

        var formView = FormViewFactory.Create(formElement);
        formView.GridView.CurrentOrder = OrderByData.FromString($"{Options.CreatedColumnName} DESC");


        formView.GridView.OnRenderCellAsync += (sender, args) =>
        {
            string? message = string.Empty;
            if (args.Field.Name.Equals(Options.MessageColumnName))
            {
                message = args.DataRow[Options.MessageColumnName].ToString()?.Replace("\n", "<br>");
            }
            else
            {
                if (args.Sender is HtmlComponent component)
                {
                    message = component.GetHtml();
                }
            
            }
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
        string sql = $"TRUNCATE TABLE {Options.TableName}";

        await EntityRepository.SetCommandAsync(new DataAccessCommand(sql));

        return RedirectToAction("Index");
    }
}