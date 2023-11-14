using System;
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

internal class GridViewFactory(IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IEncryptionService encryptionService,
        DataItemService dataItemService,
        ExpressionsService expressionsService,
        FieldsService fieldsService,
        FormValuesService formValuesService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IGridEventHandlerResolver gridEventHandlerResolver,
        IComponentFactory componentFactory)
    : IFormElementComponentFactory<JJGridView>
{
    private FieldsService FieldsService { get; } = fieldsService;
    private FormValuesService FormValuesService { get; } = formValuesService;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IGridEventHandlerResolver GridEventHandlerResolver { get; } = gridEventHandlerResolver;
    private IComponentFactory ComponentFactory { get; } = componentFactory;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private DataItemService DataItemService { get; } = dataItemService;
    private IHttpContext CurrentContext { get; } = currentContext;


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

        var eventHandler = GridEventHandlerResolver.GetGridEventHandler(formElement.Name);
        
        SetGridOptions(gridView, formElement.Options);
        
        if(eventHandler != null)
            SetGridEvents(gridView, eventHandler);
        
        return gridView;
    }

    private static void SetGridEvents(JJGridView gridView, IGridEventHandler eventHandler)
    {
        gridView.OnDataLoadAsync += eventHandler.OnDataLoadAsync;
        gridView.OnRenderActionAsync += eventHandler.OnRenderActionAsync;
        gridView.OnRenderCellAsync += eventHandler.OnRenderCellAsync;
        gridView.OnRenderSelectedCellAsync += eventHandler.OnRenderSelectedCellAsync;
    }

    private static bool IsMethodImplemented(Type type, string methodName)
    {
        var method = type.GetMethod(methodName);

        return method is not null;
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