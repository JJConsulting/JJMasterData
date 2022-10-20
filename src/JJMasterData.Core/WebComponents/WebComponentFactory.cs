using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using System;
using System.Collections;

namespace JJMasterData.Core.WebComponents
{
    internal static class WebComponentFactory
    {

        public static JJDataPanel CreateDataPanel(string elementName)
        {
            var dataPanel = new JJDataPanel();
            SetDataPanelParams(dataPanel, elementName);
            return dataPanel;
        }

        public static JJDataPanel CreateFormView()
        {
            //TODO: Change FromView contructor
            throw new NotImplementedException();
        }

        public static JJGridView CreateGridView(string elementName)
        {
            var grid = new JJGridView();
            SetGridViewParams(grid, elementName);

            return grid;
        }

        internal static void SetDataPanelParams(JJDataPanel dataPanel, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName));

            var dicParser = GetDictionary(elementName);
            var formElement = dicParser.GetFormElement();

            dataPanel.FormElement = formElement;
            dataPanel.Name = "pnl_" + elementName.ToLower();
            dataPanel.RenderPanelGroup = formElement.Panels.Count > 0;
            dataPanel.UISettings = dicParser.UIOptions.Form;
        }



        internal static void SetGridViewParams(JJGridView grid, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName), "Nome do dicionário nao pode ser vazio");

            var dicParser = GetDictionary(elementName);
            grid.Name = "jjview" + elementName.ToLower();
            grid.FormElement = dicParser.GetFormElement();
            grid.SetGridOptions(dicParser.UIOptions.Grid);
        }


        private static DicParser GetDictionary(string elementName)
        {
            var dicDao = new DictionaryDao();
            return dicDao.GetDictionary(elementName);
        }

    }
}
