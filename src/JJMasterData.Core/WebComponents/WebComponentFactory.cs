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

    }
}
