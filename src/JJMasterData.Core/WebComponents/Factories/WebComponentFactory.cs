namespace JJMasterData.Core.WebComponents.Factories;

public class WebComponentFactory
{
    private FormViewFactory FormViewFactory { get; }
    private GridViewFactory GridViewFactory { get; }
    private DataPanelFactory DataPanelFactory { get; }
    public DataImpFactory DataImpFactory { get; }

    public WebComponentFactory(
        FormViewFactory formViewFactory,
        GridViewFactory gridViewFactory,
        DataPanelFactory dataPanelFactory,
        DataImpFactory dataImpFactory)
    {
        FormViewFactory = formViewFactory;
        GridViewFactory = gridViewFactory;
        DataPanelFactory = dataPanelFactory;
        DataImpFactory = dataImpFactory;
    }
    
    public JJDataPanel CreateDataPanel(string elementName)
    {
        return DataPanelFactory.CreateDataPanel(elementName);
    }

    public JJGridView CreateGridView(string elementName)
    {
        return GridViewFactory.CreateGridView(elementName);
    }

    public JJFormView CreateFormView(string elementName)
    {
        return FormViewFactory.CreateFormView(elementName);
    }

    public JJDataImp CreateDataImp(string elementName)
    {
        return DataImpFactory.CreateDataImp(elementName);
    }
    
}