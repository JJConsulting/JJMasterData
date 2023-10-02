using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class GridViewFactory : IFormElementComponentFactory<JJGridView>
{
    private FieldsService FieldsService { get; }
    private FormValuesService FormValuesService { get; }
    private ExpressionsService ExpressionsService { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IGridEventHandlerFactory GridEventHandlerFactory { get; }
    private IComponentFactory ComponentFactory { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private DataItemService DataItemService { get; }
    private IHttpContext CurrentContext { get; }

    public GridViewFactory(IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IEncryptionService encryptionService,
        DataItemService dataItemService,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IGridEventHandlerFactory gridEventHandlerFactory,
        IComponentFactory componentFactory)
    {
        CurrentContext = currentContext;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        GridEventHandlerFactory = gridEventHandlerFactory;
        ComponentFactory = componentFactory;
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        DataItemService = dataItemService;
    }


    public JJGridView Create(FormElement formElement)
    {
        var gridView = new JJGridView(
            formElement, 
            CurrentContext,
            EntityRepository,
            EncryptionService, 
            DataItemService, 
            ExpressionsService, 
            FieldsService, 
            FormValuesService,
            StringLocalizer,
            ComponentFactory);

        var eventHandler = GridEventHandlerFactory.GetGridEventHandler(formElement.Name);
        
        SetGridOptions(gridView, formElement.Options);
        
        if(eventHandler != null)
            SetGridEvents(gridView, eventHandler);
        
        return gridView;
    }

    private static void SetGridEvents(JJGridView gridView, IGridEventHandler eventHandler)
    {
        if (IsMethodImplemented(eventHandler, nameof(eventHandler.OnDataLoad)))
        {
            gridView.OnDataLoad += eventHandler.OnDataLoad;
        }

        if (IsMethodImplemented(eventHandler, nameof(eventHandler.OnDataLoadAsync)))
        {
            gridView.OnDataLoadAsync += eventHandler.OnDataLoadAsync;
        }

        if (IsMethodImplemented(eventHandler, nameof(eventHandler.OnRenderAction)))
        {
            gridView.OnRenderAction += eventHandler.OnRenderAction;
        }

        if (IsMethodImplemented(eventHandler, nameof(eventHandler.OnRenderCell)))
        {
            gridView.OnRenderCell += eventHandler.OnRenderCell;
        }

        if (IsMethodImplemented(eventHandler, nameof(eventHandler.OnRenderSelectedCell)))
        {
            gridView.OnRenderSelectedCell += eventHandler.OnRenderSelectedCell;
        }
    }

    private static bool IsMethodImplemented(IGridEventHandler eventHandler, string methodName)
    {
        var type = eventHandler.GetType();
        var method = type.GetMethod(methodName);

        return method!.DeclaringType != typeof(GridEventHandlerBase);
    }

    public JJGridView Create(DataTable dataTable)
    {
        var gridView = Create(new FormElement(dataTable));

        return gridView;
    }

    public async Task<JJGridView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);

        var gridView = Create(formElement);

        return gridView;
    }

    public JJGridView Create<T>(IEnumerable<T> list)
    {
        var dataTable = EnumerableHelper.ConvertToDataTable(list);
        var grid = Create(dataTable);
        return grid;
    }

    internal void SetGridOptions(JJGridView grid, FormElementOptions options)
    {
        var gridOptions = options.Grid;

        SetGridUiOptions(grid, gridOptions);
    }
    internal void SetGridUiOptions(JJGridView grid, GridUI gridOptions)
    {
        grid.EnableSorting = gridOptions.EnableSorting;
        grid.EnableMultiSelect = gridOptions.EnableMultSelect;
        grid.MaintainValuesOnLoad = gridOptions.MaintainValuesOnLoad;
        grid.ShowPagging = gridOptions.ShowPagging;
        grid.ShowToolbar = gridOptions.ShowToolBar;

        if (!GridFormSettings.HasFormValues(grid.CurrentContext) || !grid.ShowToolbar || !grid.ConfigAction.IsVisible)
        {
            GridSettings settings = null;
            if (grid.MaintainValuesOnLoad)
                settings = CurrentContext.Session.GetSessionValue<GridSettings>($"jjcurrentui_{grid.FormElement.Name}");

            if (settings == null)
            {
                settings = grid.CurrentSettings;
                settings.ShowRowHover = gridOptions.ShowRowHover;
                settings.ShowRowStriped = gridOptions.ShowRowStriped;
                settings.ShowBorder = gridOptions.ShowBorder;
                settings.RecordsPerPage = gridOptions.TotalPerPage;
                settings.TotalPaginationButtons = gridOptions.TotalPaggingButton;
                settings.IsHeaderFixed = gridOptions.HeaderFixed;
            }

            grid.CurrentSettings = settings;
        }

        grid.ShowHeaderWhenEmpty = gridOptions.ShowHeaderWhenEmpty;
        grid.EmptyDataText = gridOptions.EmptyDataText;
    }
}