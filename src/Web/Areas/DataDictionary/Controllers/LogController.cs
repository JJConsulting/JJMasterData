using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController : DataDictionaryController
{
    private DbLoggerOptions Options { get; }

    private IFormElementComponentFactory<JJFormView> FormViewFactory { get; }
    private LoggerFormElementFactory LoggerFormElementFactory { get; }
    private IEntityRepository EntityRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public LogController(IFormElementComponentFactory<JJFormView> formViewFactory,
        LoggerFormElementFactory loggerFormElementFactory, IEntityRepository entityRepository,
        IStringLocalizer<JJMasterDataResources> stringLocalizer, IOptions<DbLoggerOptions> options)
    {
        FormViewFactory = formViewFactory;
        LoggerFormElementFactory = loggerFormElementFactory;
        EntityRepository = entityRepository;
        StringLocalizer = stringLocalizer;
        Options = options.Value;
    }

    public async Task<IActionResult> Index()
    {
        var formElement = LoggerFormElementFactory.GetFormElement();

        if (!await EntityRepository.TableExistsAsync(Options.TableName))
        {
            await EntityRepository.CreateDataModelAsync(formElement);
        }

        var formView = FormViewFactory.Create(formElement);
        formView.GridView.CurrentOrder = OrderByData.FromString($"{Options.CreatedColumnName} DESC");

        formView.GridView.OnRenderCell += (sender, args) =>
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
        };
        
        var result = await formView.GetResultAsync();

        if (result.IsActionResult())
            return result.ToActionResult();

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