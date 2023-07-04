using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class GridViewFactory
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IHttpSession Session { get; }

    public GridViewFactory(IDataDictionaryRepository dataDictionaryRepository, IHttpSession session)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        Session = session;
    }

    public JJGridView CreateGridView(DataTable dataTable)
    {
        var gridView = new JJGridView(dataTable);

        return gridView;
    }
    
    public JJGridView CreateGridView(FormElement formElement)
    {
        var gridView = new JJGridView(formElement, true);

        SetGridOptions(gridView, formElement.Options);
        
        return gridView;
    }
    
    public async Task<JJGridView> CreateGridViewAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

        var gridView = new JJGridView(formElement, true);

        SetGridOptions(gridView, formElement.Options);
        
        return gridView;
    }
    
    public static JJGridView CreateGridView<T>(IEnumerable<T> list)
    {
        var data = EnumerableHelper.ConvertToDataTable(list);
        var grid = new JJGridView(data);
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

        if (!GridSettings.HasFormValues(grid.CurrentContext) | !grid.ShowToolbar | !grid.ConfigAction.IsVisible)
        {
            GridSettings settings = null;
            if (grid.MaintainValuesOnLoad && grid.FormElement != null)
                settings = Session.GetSessionValue<GridSettings>($"jjcurrentui_{grid.FormElement.Name}");

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