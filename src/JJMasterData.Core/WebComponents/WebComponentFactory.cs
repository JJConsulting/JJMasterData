using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JJMasterData.Core.WebComponents
{
    internal static class WebComponentFactory
    {
        public static JJDataPanel CreateDataPanel(string elementName)
        {
            return DataPanelFactory.CreateDataPanel(elementName);
        }

        public static JJGridView CreateGridView(string elementName)
        {
            return GridViewFactory.CreateGridView(elementName);
        }

        public static JJFormView CreateFormView(string elementName)
        {
            return FormFactory.CreateFormView(elementName);
        }

        public static JJDataImp CreateDataImp(string elementName)
        {
            var dataImp = new JJDataImp();
            SetDataImpParams(dataImp, elementName);
            return dataImp;
        }
        
        internal static void SetDataImpParams(JJDataImp dataPanel, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName));

            var dicDao = DictionaryRepositoryFactory.GetInstance();
            var dicParser = dicDao.GetDictionary(elementName);
            dataPanel.FormElement = dicParser.GetFormElement();
            dataPanel.ProcessOptions = dicParser.UIOptions.ToolBarActions.ImportAction.ProcessOptions;
        }

    }
}
