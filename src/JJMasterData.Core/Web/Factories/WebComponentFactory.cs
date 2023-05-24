using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DI;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories
{
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

        public static JJDataImp CreateDataImp(string elementName)
        {
            var dataImp = new JJDataImp();
            SetDataImpParams(dataImp, elementName);
            return dataImp;
        }
        
        internal static void SetDataImpParams(JJDataImp dataImp, string elementName)
        {
            if (string.IsNullOrEmpty(elementName))
                throw new ArgumentNullException(nameof(elementName));

            var dicDao = JJServiceCore.DataDictionaryRepository;
            var formElement = dicDao.GetMetadata(elementName);
            
            var dataContext = new DataContext(DataContextSource.Upload, DataHelper.GetCurrentUserId(null));
            
            var formEvent = JJServiceCore.FormEventResolver.GetFormEvent(elementName);
            formEvent?.OnFormElementLoad(dataContext, new FormElementLoadEventArgs(formElement));

            if (formEvent != null) 
                dataImp.OnBeforeImport += formEvent.OnBeforeImport;

            dataImp.FormElement = formElement;
            dataImp.ProcessOptions = formElement.Options.ToolbarActions.ImportAction.ProcessOptions;
        }
        
        internal static void SetDataImpParams(JJDataImp dataImp, FormElement formElement)
        {
            if (formElement is null)
                throw new ArgumentNullException(nameof(formElement));

            var formEvent = JJServiceCore.FormEventResolver.GetFormEvent(formElement.Name);

            if (formEvent != null) 
                dataImp.OnBeforeImport += formEvent.OnBeforeImport;
            
            dataImp.FormElement = formElement;
        }

    }
}
