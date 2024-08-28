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
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridViewFactory(IHttpContext currentContext,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IEncryptionService encryptionService,
        DataItemService dataItemService,
        ExpressionsService expressionsService,
        FieldFormattingService fieldFormattingService,
        FieldValuesService fieldValuesService,
        FieldValidationService fieldValidationService,
        FormValuesService formValuesService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IGridEventHandlerResolver gridEventHandlerResolver,
        UrlRedirectService urlRedirectService,
        HtmlTemplateService htmlTemplateService,
        ILoggerFactory loggerFactory,
        IComponentFactory componentFactory)
    : IFormElementComponentFactory<JJGridView>
{
    public JJGridView Create(FormElement formElement)
    {
        var gridView = new JJGridView(
            formElement, 
            currentContext,
            entityRepository,
            encryptionService, 
            dataItemService, 
            expressionsService, 
            formValuesService,
            fieldFormattingService,
            fieldValuesService,
            fieldValidationService,
            stringLocalizer,
            urlRedirectService,
            htmlTemplateService,
            loggerFactory.CreateLogger<JJGridView>(),
            componentFactory);

        var eventHandler = gridEventHandlerResolver.GetGridEventHandler(formElement.Name);
        
        SetGridOptions(gridView, formElement.Options);
        
        if(eventHandler != null)
            SetGridEvents(gridView, eventHandler);
        
        return gridView;
    }

    private static void SetGridEvents(JJGridView gridView, IGridEventHandler eventHandler)
    {
        gridView.OnDataLoadAsync += eventHandler.OnDataLoadAsync;
        gridView.OnRenderActionAsync += eventHandler.OnRenderActionAsync;
        gridView.OnRenderToolbarActionAsync += eventHandler.OnRenderToolbarActionAsync;
        gridView.OnRenderCellAsync += eventHandler.OnRenderCellAsync;
        gridView.OnFilterLoadAsync += eventHandler.OnFilterLoadAsync;
        gridView.OnRenderSelectedCellAsync += eventHandler.OnRenderSelectedCellAsync;
        gridView.OnRenderRowAsync += eventHandler.OnRenderRowAsync;
    }

    public JJGridView Create(DataTable dataTable)
    {
        var gridView = Create(new FormElement(dataTable));

        return gridView;
    }

    public async ValueTask<JJGridView> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);

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
        grid.EnableMultiSelect = gridOptions.EnableMultiSelect;
        grid.MaintainValuesOnLoad = gridOptions.MaintainValuesOnLoad;
        grid.ShowPaging = gridOptions.ShowPagging;
        grid.ShowToolbar = gridOptions.ShowToolBar;
        
        if (!grid.GridSettingsForm.HasFormValues() || !grid.ShowToolbar || !grid.ConfigAction.IsVisible)
        {
            GridSettings settings = null;
            if (grid.MaintainValuesOnLoad)
                settings = currentContext.Session.GetSessionValue<GridSettings>($"jjcurrentui_{grid.FormElement.Name}");

            if (settings == null)
            {
                settings = grid.CurrentSettings;
                settings.ShowRowHover = gridOptions.ShowRowHover;
                settings.ShowRowStriped = gridOptions.ShowRowStriped;
                settings.ShowBorder = gridOptions.ShowBorder;
                settings.RecordsPerPage = gridOptions.RecordsPerPage;
                settings.TotalPaginationButtons = gridOptions.TotalPaggingButton;
                settings.IsHeaderFixed = gridOptions.HeaderFixed;
                settings.IsCompact = gridOptions.IsCompact;
            }

            grid.CurrentSettings = settings;
        }

        grid.ShowHeaderWhenEmpty = gridOptions.ShowHeaderWhenEmpty;
        grid.EmptyDataText = gridOptions.EmptyDataText;
    }
}