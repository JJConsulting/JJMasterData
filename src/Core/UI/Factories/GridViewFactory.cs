using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Factories;

public class GridViewFactory
{
    private IFieldsService FieldsService { get; }
    private IExpressionsService ExpressionsService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private DataExportationFactory DataExportationFactory { get; }
    private DataImportationFactory DataImportationFactory { get; }
    private FieldControlFactory FieldControlFactory { get; }
    private IEntityRepository EntityRepository { get; }
    private ScriptsHelper ScriptsHelper { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IHttpContext CurrentContext { get; }


    public GridViewFactory(
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        JJMasterDataEncryptionService encryptionService,
        IFieldsService fieldsService,
        ScriptsHelper scriptsHelper,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        DataExportationFactory dataExportationFactory,
        DataImportationFactory dataImportationFactory,
        FieldControlFactory fieldControlFactory)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        CurrentContext = currentContext;
        FieldsService = fieldsService;
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        DataExportationFactory = dataExportationFactory;
        DataImportationFactory = dataImportationFactory;
        FieldControlFactory = fieldControlFactory;
        EntityRepository = entityRepository;
        ScriptsHelper = scriptsHelper;
    }

    public JJGridView CreateGridView()
    {
        var gridView = new JJGridView(CurrentContext, EntityRepository, ExpressionsService, EncryptionService,
            FieldsService, ScriptsHelper, StringLocalizer, DataExportationFactory, DataImportationFactory,
            FieldControlFactory);

        return gridView;
    }
    
    public JJGridView CreateGridView(FormElement formElement)
    {
        var gridView = new JJGridView(formElement,CurrentContext, EntityRepository, ExpressionsService, EncryptionService,
            FieldsService, ScriptsHelper, StringLocalizer, DataExportationFactory, DataImportationFactory,
            FieldControlFactory);
        
        SetGridOptions(gridView,formElement.Options);
        
        return gridView;
    }

    public JJGridView CreateGridView(DataTable dataTable)
    {
        var gridView = CreateGridView(new FormElement(dataTable));

        return gridView;
    }

    public async Task<JJGridView> CreateGridViewAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

        var gridView = CreateGridView(formElement);

        return gridView;
    }

    public JJGridView CreateGridView<T>(IEnumerable<T> list)
    {
        var dataTable = EnumerableHelper.ConvertToDataTable(list);
        var grid = CreateGridView(dataTable);
        return grid;
    }

    internal void SetGridOptions(JJGridView grid, FormElementOptions options)
    {
        var gridOptions = options.Grid;

        if (gridOptions == null)
            throw new ArgumentNullException(nameof(gridOptions), "Grid Options");

        SetGridUiOptions(grid, gridOptions);

        foreach (var action in options.GridTableActions)
        {
            grid.GridActions.Add(action);
        }

        foreach (var action in options.FormToolbarActions)
        {
            grid.ToolBarActions.Add(action);
        }
    }

    internal void SetGridUiOptions(JJGridView grid, GridUI gridOptions)
    {
        grid.EnableAjax = true;
        grid.EnableSorting = gridOptions.EnableSorting;
        grid.EnableMultiSelect = gridOptions.EnableMultSelect;
        grid.MaintainValuesOnLoad = gridOptions.MaintainValuesOnLoad;
        grid.ShowPagging = gridOptions.ShowPagging;
        grid.ShowToolbar = gridOptions.ShowToolBar;

        if (!GridFormSettings.HasFormValues(grid.CurrentContext) | !grid.ShowToolbar | !grid.ConfigAction.IsVisible)
        {
            GridSettings settings = null;
            if (grid.MaintainValuesOnLoad && grid.FormElement != null)
                settings = CurrentContext.Session.GetSessionValue<GridSettings>($"jjcurrentui_{grid.FormElement.Name}");

            if (settings == null)
            {
                settings = grid.CurrentSettings;
                settings.ShowRowHover = gridOptions.ShowRowHover;
                settings.ShowRowStriped = gridOptions.ShowRowStriped;
                settings.ShowBorder = gridOptions.ShowBorder;
                settings.TotalPerPage = gridOptions.TotalPerPage;
                settings.TotalPaginationButtons = gridOptions.TotalPaggingButton;
                settings.IsHeaderFixed = gridOptions.HeaderFixed;
            }

            grid.CurrentSettings = settings;
        }

        grid.ShowHeaderWhenEmpty = gridOptions.ShowHeaderWhenEmpty;
        grid.EmptyDataText = gridOptions.EmptyDataText;
    }
}