using System.Collections.Generic;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.Facades;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class WebComponentFactory
{
    private FormViewFactory _formViewFactory;
    private GridViewFactory _gridViewFactory;
    private DataPanelFactory _dataPanelFactory;
    private DataImpFactory _dataImpFactory;

    public IEnumerable<IExportationWriter> ExportationWriters { get; }

    public CoreServicesFacade CoreServicesFacade { get; }

    public RepositoryServicesFacade RepositoryServicesFacade { get; }

    public IHttpContext HttpContext { get; }

    private GridViewFactory GridViewFactory => _gridViewFactory ??=
        new GridViewFactory(HttpContext, RepositoryServicesFacade, CoreServicesFacade, ExportationWriters);

    private FormViewFactory FormViewFactory => _formViewFactory ??= new FormViewFactory(
        HttpContext,
        RepositoryServicesFacade,
        CoreServicesFacade,
        ExportationWriters);

    private DataPanelFactory DataPanelFactory => _dataPanelFactory ??=
        new DataPanelFactory(HttpContext, RepositoryServicesFacade, CoreServicesFacade);

    public DataImpFactory DataImpFactory => _dataImpFactory ??=
        new DataImpFactory(HttpContext, RepositoryServicesFacade, CoreServicesFacade);

    public WebComponentFactory(
        IHttpContext httpContext,
        RepositoryServicesFacade repositoryServicesFacade,
        CoreServicesFacade coreServicesFacade,
        IEnumerable<IExportationWriter> exportationWriters)
    {
        HttpContext = httpContext;
        RepositoryServicesFacade = repositoryServicesFacade;
        CoreServicesFacade = coreServicesFacade;
        ExportationWriters = exportationWriters;
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