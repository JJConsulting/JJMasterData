using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DI;
using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.Web.Components
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

            var dictionaryRepository = JJServiceCore.DataDictionaryRepository;
            var metadata = dictionaryRepository.GetMetadata(elementName);
            grid.Name = "jjview" + elementName.ToLower();
            grid.FormElement = metadata.GetFormElement();
            SetGridOptions(grid, metadata.MetadataOptions.Grid);
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
                    settings = JJSession.GetSessionValue<GridSettings>($"jjcurrentui_{grid.FormElement.Name}");

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
}
