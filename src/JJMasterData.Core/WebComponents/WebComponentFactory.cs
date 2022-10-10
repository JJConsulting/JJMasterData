using JJMasterData.Core.DataDictionary.DictionaryDAL;
using System;

namespace JJMasterData.Core.WebComponents
{
    internal static class WebComponentFactory
    {

        public static JJDataPanel CreateDataPanel()
        {
            //TODO: Change FromView contructor
            throw new NotImplementedException();
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
