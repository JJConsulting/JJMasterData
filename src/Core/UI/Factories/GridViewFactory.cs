using System;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.Web.Factories;

internal static class GridViewFactory
{
    public static JJGridView CreateGridView(string elementName)
    {
        var grid = new JJGridView();
        SetGridViewParams(grid, elementName);
        return grid;
    }

    public static JJGridView CreateGridView(FormElement formElement)
    {
        var grid = new JJGridView(formElement);
        return grid;
    }

    public static JJGridView CreateGridView(DataTable dataTable)
    {
        var grid = new JJGridView(dataTable);
        return grid;
    }

    public static JJGridView CreateGridView<T>(IEnumerable<T> list)
    {
        var data = EnumerableHelper.ConvertToDataTable(list);
        var grid = new JJGridView(data);
        return grid;
    }

    internal static void SetGridViewParams(JJGridView grid, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName), "Nome do dicionário nao pode ser vazio");

        var dictionaryRepository = JJService.Provider.GetScopedDependentService<IDataDictionaryRepository>();
        var metadata = dictionaryRepository.GetMetadata(elementName);
        grid.Name = "jjview" + elementName.ToLower();
        grid.FormElement = metadata;
        SetGridOptions(grid, metadata.Options.Grid);
    }

    internal static void SetGridViewParams(JJGridView grid, FormElement formElement)
    {
        grid.Name = "jjview" + formElement.Name.ToLower();
        grid.FormElement = formElement;
        SetGridOptions(grid, formElement.Options.Grid);
    }
    
    internal static void SetGridOptions(JJGridView grid, GridUI options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options), "Grid Options");

        grid.EnableAjax = true;
        grid.EnableSorting = options.EnableSorting;
        grid.EnableMultSelect = options.EnableMultSelect;
        grid.MaintainValuesOnLoad = options.MaintainValuesOnLoad;
        grid.ShowPagging = options.ShowPagging;
        grid.ShowToolbar = options.ShowToolBar;

        if (!GridSettings.HasFormValues(grid.CurrentContext) | !grid.ShowToolbar | !grid.ConfigAction.IsVisible)
        {
            GridSettings settings = null;
            if (grid.MaintainValuesOnLoad && grid.FormElement != null)
                settings = JJHttpContext.GetInstance().Session.GetSessionValue<GridSettings>($"jjcurrentui_{grid.FormElement.Name}");

            if (settings == null)
            {
                settings = grid.CurrentSettings;
                settings.ShowRowHover = options.ShowRowHover;
                settings.ShowRowStriped = options.ShowRowStriped;
                settings.ShowBorder = options.ShowBorder;
                settings.TotalPerPage = options.TotalPerPage;
                settings.TotalPaginationButtons = options.TotalPaggingButton;
                settings.IsHeaderFixed = options.HeaderFixed;
            }

            grid.CurrentSettings = settings;
        }

        grid.ShowHeaderWhenEmpty = options.ShowHeaderWhenEmpty;
        grid.EmptyDataText = options.EmptyDataText;
    }


}