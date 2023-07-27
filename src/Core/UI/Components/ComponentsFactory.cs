using JJMasterData.Core.UI.Components.GridView;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JJMasterData.Core.Web.Factories;

public class ComponentsFactory
{
    private readonly IServiceProvider _provider;

    public ComponentsFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public AuditLogViewFactory AuditLog => _provider.GetRequiredService<AuditLogViewFactory>();
    public ControlsFactory Controls => _provider.GetRequiredService<ControlsFactory>();
    public DataExportationFactory DataExportation => _provider.GetRequiredService<DataExportationFactory>();
    public DataImportationFactory DataImportation => _provider.GetRequiredService<DataImportationFactory>();
    public DataPanelFactory DataPanel => _provider.GetRequiredService<DataPanelFactory>();
    public FormViewFactory FormView => _provider.GetRequiredService<FormViewFactory>();
    public FormUploadFactory FormUpload => _provider.GetRequiredService<FormUploadFactory>();
    public GridViewFactory GridView => _provider.GetRequiredService<GridViewFactory>();

}