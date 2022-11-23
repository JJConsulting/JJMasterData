using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JJMasterData.Core.WebComponents
{
    internal static class DataPanelFactory
    {
        public static JJDataPanel CreateDataPanel(string elementName)
        {
            var dataPanel = new JJDataPanel();
            SetDataPanelParams(dataPanel, elementName);
            return dataPanel;
        }

        internal static void SetDataPanelParams(JJDataPanel dataPanel, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName));

            var dicDao = DictionaryRepositoryFactory.GetInstance();
            var dicParser = dicDao.GetMetadata(elementName);
            var formElement = dicParser.GetFormElement();

            SetDataPanelParams(dataPanel, formElement);
            dataPanel.UISettings = dicParser.UIOptions.Form;
        }

        internal static void SetDataPanelParams(JJDataPanel dataPanel, FormElement formElement)
        {
            if (formElement == null)
                throw new ArgumentNullException(nameof(formElement));

            dataPanel.FormElement = formElement;
            dataPanel.Name = "pnl_" + formElement.Name.ToLower();
            dataPanel.RenderPanelGroup = formElement.Panels.Count > 0;
        }

    }
}
