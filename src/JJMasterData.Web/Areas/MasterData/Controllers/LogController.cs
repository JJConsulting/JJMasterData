using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
[Authorize(Policy = "Log")]
public class LogController : Controller
{
    private Logger Logger { get; set; }

    public LogController()
    {
        Logger = new Logger();
    }

    public ActionResult Index()
    {
        if (Logger.Options.WriteInDatabase == LoggerOption.None)
            throw new DataDictionaryException(Translate.Key("Configuration for logging to the database is not enabled"));

        if (!Logger.LogTableExists())
        {
            var factory = new Factory();
            factory.CreateDataModel(Logger.GetElement());
        }

        var gridView = GetLoggingGridView(Logger);
        return View(gridView);
    }

    [HttpGet]
    public ActionResult ClearAll()
    {
        var logger = new Logger();
        logger.ClearLog();

        GetLoggingGridView(logger);

        return RedirectToAction("Index");
    }

    private JJGridView GetLoggingGridView(Logger logger)
    {
        var f = new FormElement(logger.GetElement())
        {
            Title = Translate.Key("Application Log"),
            SubTitle = string.Empty
        };

        var tipo = f.Fields[Logger.Options.Table.LevelColumnName];
        tipo.Component = FormComponent.ComboBox;
        tipo.DataItem.Items.Add(new DataItemValue("I", "Info"));
        tipo.DataItem.Items.Add(new DataItemValue("W", "Alerta"));
        tipo.DataItem.Items.Add(new DataItemValue("E", "Erro"));

        var gridView = new JJGridView(f);
        gridView.CurrentOrder = $"{Logger.Options.Table.DateColumnName} DESC";
        gridView.OnRenderCell += OnRenderCell!;

        var btnClearAll = new UrlRedirectAction
        {
            Name = "btnClearLog",
            Icon = IconType.Trash,
            Text = Translate.Key("Clear Log"),
            ShowAsButton = true,
            UrlRedirect = Url.Action("ClearAll")
        };
            
        gridView.AddToolBarAction(btnClearAll);

        return gridView;
    }
        

    private void OnRenderCell(object sender, GridCellEventArgs e)
    {
        string msg;
        if (e.Field.Name.Equals(Logger.Options.Table.ContentColumnName))
        {
            msg = e.DataRow[Logger.Options.Table.ContentColumnName].ToString().Replace("\r\n", "<br>");
        }
        else
        {
            msg = e.Sender.GetHtml();
        }

        e.HtmlResult = msg;
    }

}