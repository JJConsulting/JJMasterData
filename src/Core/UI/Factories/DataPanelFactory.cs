using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

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

        var dicDao = dataPanel.DataDictionaryRepository;
        var formElement = dicDao.GetMetadata(elementName);

        SetDataPanelParams(dataPanel, formElement);
        dataPanel.FormUI = formElement.Options.Form;
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