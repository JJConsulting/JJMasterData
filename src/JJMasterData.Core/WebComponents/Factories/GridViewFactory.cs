using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.WebComponents.Factories;

public class GridViewFactory
{
    private IHttpContext HttpContext { get; }
    private RepositoryServicesFacade RepositoryServicesFacade { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private ILoggerFactory LoggerFactory { get; }
    private IBackgroundTask BackgroundTask { get; }
    private IOptions<JJMasterDataCoreOptions> Options { get; }
    private AuditLogService AuditLogService { get; }
    private IEnumerable<IExportationWriter> ExportationWriters { get;  }
        
    public GridViewFactory(
        IHttpContext httpContext, 
        RepositoryServicesFacade repositoryServicesFacade, 
        JJMasterDataEncryptionService encryptionService,
        ILoggerFactory loggerFactory,
        IEnumerable<IExportationWriter> exportationWriters, 
        IBackgroundTask backgroundTask, 
        IOptions<JJMasterDataCoreOptions> options, 
        AuditLogService auditLogService)
    {
        HttpContext = httpContext;
        ExportationWriters = exportationWriters;
        BackgroundTask = backgroundTask;
        Options = options;
        AuditLogService = auditLogService;
        RepositoryServicesFacade = repositoryServicesFacade;
        EncryptionService = encryptionService;
        LoggerFactory = loggerFactory;
    }

    public JJGridView CreateGridView() => new (HttpContext,RepositoryServicesFacade,EncryptionService,LoggerFactory,BackgroundTask,ExportationWriters,Options, AuditLogService);
    
    public JJGridView CreateGridView(string elementName)
    {
        var grid = CreateGridView();
        SetGridViewParams(grid, elementName);
        return grid;
    }

    public JJGridView CreateGridView(FormElement formElement)
    {
        var grid = CreateGridView();

        grid.FormElement = formElement;

        return grid;
    }
    
    public JJGridView CreateGridView(DataTable dataTable)
    {
        var grid = CreateGridView();

        grid.FormElement = new FormElement(dataTable);

        return grid;
    }
    
    internal static void SetGridViewParams(JJGridView gridView)
    {
        gridView.Name = "jjview";
        gridView.ShowTitle = true;
        gridView.EnableFilter = true;
        gridView.EnableAjax = true;
        gridView.EnableSorting = true;
        gridView.ShowHeaderWhenEmpty = true;
        gridView.ShowPagging = true;
        gridView.ShowToolbar = true;
        gridView.EmptyDataText = "No records found";
        gridView.AutoReloadFormFields = true;
        gridView.RelationValues = new Hashtable();
        gridView.TitleSize = HeadingSize.H1;
    }
    
    internal void SetGridViewParams(JJGridView grid, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), "elementName cannot be null or empty.");

        var metadata = RepositoryServicesFacade.DataDictionaryRepository.GetMetadata(elementName);
        grid.Name = "jjview" + elementName.ToLower();
        grid.FormElement = metadata.GetFormElement();
        SetGridOptions(grid, metadata.UIOptions.Grid);
    }

    internal static void SetGridOptions(JJGridView grid, UIGrid options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options), "Grid Options");

        grid.EnableAjax = true;
        grid.EnableSorting = options.EnableSorting;
        grid.EnableMultSelect = options.EnableMultSelect;
        grid.MaintainValuesOnLoad = options.MaintainValuesOnLoad;
        grid.ShowPagging = options.ShowPagging;
        grid.ShowToolbar = options.ShowToolBar;

        if (!GridUI.HasFormValues(grid.HttpContext) | !grid.ShowToolbar | !grid.ConfigAction.IsVisible)
        {
            GridUI ui = null;
            if (grid.MaintainValuesOnLoad && grid.FormElement != null)
                ui = grid.HttpContext.Session.GetSessionValue<GridUI>($"jjcurrentui_{grid.FormElement.Name}");

            if (ui == null)
            {
                ui = grid.CurrentUI;
                ui.ShowRowHover = options.ShowRowHover;
                ui.ShowRowStriped = options.ShowRowStriped;
                ui.ShowBorder = options.ShowBorder;
                ui.TotalPerPage = options.TotalPerPage;
                ui.TotalPaginationButtons = options.TotalPaggingButton;
                ui.IsHeaderFixed = options.HeaderFixed;
            }

            grid.CurrentUI = ui;
        }

        grid.ShowHeaderWhenEmpty = options.ShowHeaderWhenEmpty;
        grid.EmptyDataText = options.EmptyDataText;
    }


}