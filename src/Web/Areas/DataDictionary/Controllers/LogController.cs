﻿using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class LogController : DataDictionaryController
{
    private DbLoggerOptions Options { get; }
    private Element LoggerElement { get;  }
    
    private IEntityRepository EntityRepository { get; }

    public LogController(IOptions<DbLoggerOptions> options, IEntityRepository entityRepository)
    {
        EntityRepository = entityRepository;
        Options = options.Value;
        LoggerElement = DbLoggerElement.GetInstance(Options);
    }

    public ActionResult Index()
    {
        if (!EntityRepository.TableExists(Options.TableName))
        {
            EntityRepository.CreateDataModel(LoggerElement);
        }

        var formElement = GetFormElement();
        var model = new LogViewModel(formElement, ConfigureGridView);
        
        return View(model);
    }

    [HttpGet]
    public ActionResult ClearAll()
    {
        string sql = $"TRUNCATE TABLE {Options.TableName}";
        
        EntityRepository.SetCommand(sql);
        
        return RedirectToAction("Index");
    }

    private void ConfigureGridView(JJGridView gridView)
    {
        gridView.CurrentOrder = $"{Options.CreatedColumnName} DESC";
        var btnClearAll = new UrlRedirectAction
        {
            Name = "btnClearLog",
            Icon = IconType.Trash,
            Text = Translate.Key("Clear Log"),
            ShowAsButton = true,
            ConfirmationMessage = Translate.Key("Do you want to clear ALL logs?"),
            UrlRedirect = Url.Action("ClearAll")
        };

        gridView.OnRenderCell += OnRenderCell;
        gridView.AddToolBarAction(btnClearAll);
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

    private FormElement GetFormElement()
    {
        var formElement = new FormElement(LoggerElement)
        {
            Title = Translate.Key("Application Log"),
            SubTitle = string.Empty
        };

        formElement.Fields["Id"].VisibleExpression = "val:0";

        var logLevel = formElement.Fields[Options.LevelColumnName];
        logLevel.Component = FormComponent.ComboBox;

        logLevel.DataItem!.Items.Add(new DataItemValue("0", LogLevel.Trace.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("1", LogLevel.Debug.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("2", LogLevel.Information.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("3", LogLevel.Warning.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("4", LogLevel.Error.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("5", LogLevel.Critical.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("6", LogLevel.None.ToString()));
        
        return formElement;
    }


    
}