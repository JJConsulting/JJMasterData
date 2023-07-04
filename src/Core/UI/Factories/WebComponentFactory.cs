using System;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public static class WebComponentFactory
{
    public static JJDataPanel CreateDataPanel(string elementName)
    {
        return DataPanelFactory.CreateDataPanel(elementName);
    }

    public static JJGridView CreateGridView(string elementName)
    {
        return GridViewFactory.CreateGridView(elementName);
    }

    public static JJGridView CreateGridView(FormElement formElement)
    {
        return GridViewFactory.CreateGridView(formElement);
    }

    public static JJGridView CreateGridView(DataTable dataTable)
    {
        return GridViewFactory.CreateGridView(dataTable);
    }

    public static JJGridView CreateGridView<T>(IEnumerable<T> list)
    {
        return GridViewFactory.CreateGridView(list);
    }

    public static JJFormView CreateFormView(string elementName)
    {
        return FormFactory.CreateFormView(elementName);
    }

}