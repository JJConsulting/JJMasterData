using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;
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

    public LogController(IOptions<DbLoggerOptions> options, IEntityRepository entityRepository)
    {
        EntityRepository = entityRepository;
        Options = options.Value;
        LoggerElement = DbLoggerElement.GetInstance(options.Value.TableName);
    }

    public ActionResult Index()
    {
        if (EntityRepository.TableExists(Options.TableName))
        {
            var factory = new Factory();
            factory.CreateDataModel(LoggerElement);
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
        var f = new FormElement(LoggerElement)
        {
            Title = Translate.Key("Application Log"),
            SubTitle = string.Empty
        };

        var tipo = f.Fields["LogLevel"];
        tipo.Component = FormComponent.ComboBox;
        
        //TODO
        // tipo.DataItem.Items.Add(new DataItemValue("2", "Alerta"));
        // tipo.DataItem.Items.Add(new DataItemValue("3", "Erro"));

        var gridView = new JJGridView(f)
        {
            CurrentOrder = $"Log DESC"
        };
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
        // string? msg;
        // if (e.Field.Name.Equals(Logger.Options.Table.ContentColumnName))
        // {
        //     msg = e.DataRow[Logger.Options.Table.ContentColumnName].ToString()?.Replace("\r\n", "<br>");
        // }
        // else
        // {
        //     msg = e.Sender.GetHtml();
        // }
        //
        // e.HtmlResult = e.;
    }

}