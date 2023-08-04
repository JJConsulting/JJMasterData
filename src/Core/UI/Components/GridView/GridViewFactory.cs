using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JJMasterData.Core.UI.Components.GridView;

internal class GridViewFactory : IFormElementComponentFactory<JJGridView>
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IFieldsService FieldsService { get; }
    private IFormValuesService FormValuesService { get; }
    private IExpressionsService ExpressionsService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ComponentFactory Factory { get; }
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IHttpContext CurrentContext { get; }


    public GridViewFactory(
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext currentContext,
        IEntityRepository entityRepository,
        JJMasterDataUrlHelper urlHelper,
        IExpressionsService expressionsService,
        JJMasterDataEncryptionService encryptionService,
        IFieldsService fieldsService,
        IFormValuesService formValuesService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ComponentFactory factory)
    {
        UrlHelper = urlHelper;
        DataDictionaryRepository = dataDictionaryRepository;
        CurrentContext = currentContext;
        FieldsService = fieldsService;
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        Factory = factory;
        EntityRepository = entityRepository;
    }
    

    public JJGridView Create(FormElement formElement)
    {
        var gridView = new JJGridView(formElement, CurrentContext, EntityRepository, UrlHelper, ExpressionsService, EncryptionService,
            FieldsService, FormValuesService, StringLocalizer, Factory);

        SetGridOptions(gridView, formElement.Options);

        return gridView;
    }

    public JJGridView Create(DataTable dataTable)
    {
        var gridView = Create(new FormElement(dataTable));

        return gridView;
    }

    public async Task<JJGridView> CreateAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

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

        if (gridOptions == null)
            throw new ArgumentNullException(nameof(gridOptions), "Grid Options");
        
        SetGridUiOptions(grid, gridOptions);
    }

    //todo? 
    internal void SetGridUiOptions(JJGridView grid, GridUI gridOptions)
    {
        grid.EnableAjax = true;
        grid.EnableSorting = gridOptions.EnableSorting;
        grid.EnableMultiSelect = gridOptions.EnableMultSelect;
        grid.MaintainValuesOnLoad = gridOptions.MaintainValuesOnLoad;
        grid.ShowPagging = gridOptions.ShowPagging;
        grid.ShowToolbar = gridOptions.ShowToolBar;

        if (!GridFormSettings.HasFormValues(grid.CurrentContext) || !grid.ShowToolbar || !grid.ConfigAction.IsVisible)
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