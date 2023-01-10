using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;
using JJMasterData.Core.WebComponents.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
[Authorize(Policy = "Log")]
public class LogController : Controller
{
    private DbLoggerOptions Options { get; }
    private Element LoggerElement { get;  }

    private IEntityRepository EntityRepository { get; }
    private GridViewFactory GridViewFactory { get; }

    public LogController(
        IOptions<DbLoggerOptions> options,
        IEntityRepository entityRepository,
        GridViewFactory gridViewFactory)
    {
        EntityRepository = entityRepository;
        GridViewFactory = gridViewFactory;

        Options = options.Value;
        LoggerElement = DbLoggerElement.GetInstance(Options);
    }

    public ActionResult Index()
    {
        if (!EntityRepository.TableExists(Options.TableName))
        {
            EntityRepository.CreateDataModel(LoggerElement);
        }

        var gridView = GetLoggingGridView();
        return View(gridView);
    }

    [HttpGet]
    public ActionResult ClearAll()
    {
        string sql = $"TRUNCATE TABLE {Options.TableName}";
        
        EntityRepository.SetCommand(sql);
        
        return RedirectToAction("Index");
    }

    private JJGridView GetLoggingGridView()
    {
        var formElement = new FormElement(LoggerElement)
        {
            Title = Translate.Key("Application Log"),
            SubTitle = string.Empty
        };

        formElement.Fields["Id"].VisibleExpression = "val:0";

        var logLevel = formElement.Fields[Options.LevelColumnName];
        logLevel.Component = FormComponent.ComboBox;
        
        logLevel.DataItem.Items.Add(new DataItemValue("0", LogLevel.Trace.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("1", LogLevel.Debug.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("2", LogLevel.Information.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("3", LogLevel.Warning.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("4", LogLevel.Error.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("5", LogLevel.Critical.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("6", LogLevel.None.ToString()));

        var gridView = GridViewFactory.CreateGridView(formElement);
        gridView.CurrentOrder = $"{Options.CreatedColumnName} DESC";

        var btnClearAll = new UrlRedirectAction
        {
            Name = "btnClearLog",
            Icon = IconType.Trash,
            Text = Translate.Key("Clear Log"),
            ShowAsButton = true,
            UrlRedirect = Url.Action("ClearAll")
        };

        gridView.OnRenderCell += OnRenderCell;
        gridView.AddToolBarAction(btnClearAll);

        return gridView;
    }

    private void OnRenderCell(object? sender, GridCellEventArgs e)
    {
        string? message;
        if (e.Field.Name.Equals(Options.MessageColumnName))
        {
            message = e.DataRow[Options.MessageColumnName].ToString()?.Replace("\n", "<br>");
        }
        else
        {
            message = e.Sender.GetHtml();
        }

        e.HtmlResult = message;
    }
    
}