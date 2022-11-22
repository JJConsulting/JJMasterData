using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JJMasterData.Core.WebComponents
{
    internal static class GridViewFactory
    {
        public static JJGridView CreateGridView(string elementName)
        {
            var grid = new JJGridView();
            SetGridViewParams(grid, elementName);
            return grid;
        }

        internal static void SetGridViewParams(JJGridView grid, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName), "Nome do dicionário nao pode ser vazio");

            var dicDao = DictionaryRepositoryFactory.GetInstance();
            var dicParser = dicDao.GetDictionary(elementName);
            grid.Name = "jjview" + elementName.ToLower();
            grid.FormElement = dicParser.GetFormElement();
            SetGridOptions(grid, dicParser.UIOptions.Grid);
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

            if (!GridUI.HasFormValues(grid.CurrentContext) | !grid.ShowToolbar | !grid.ConfigAction.IsVisible)
            {
                GridUI ui = null;
                if (grid.MaintainValuesOnLoad && grid.FormElement != null)
                    ui = JJSession.GetSessionValue<GridUI>($"jjcurrentui_{grid.FormElement.Name}");

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
}
