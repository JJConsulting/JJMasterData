using System;

namespace JJMasterData.Core.Web.Components.Scripts;

public class ScriptsHelper
{
    private readonly Lazy<DataPanelScriptHelper> _dataPanelScriptHelper;
    private readonly Lazy<FormViewScriptHelper> _formViewScriptHelper;
    private readonly Lazy<GridViewScriptHelper> _gridViewScriptHelper;
    private readonly Lazy<DataExportationScriptHelper> _dataExportationScriptHelper;

    public DataPanelScriptHelper DataPanelScriptHelper => _dataPanelScriptHelper.Value;

    public FormViewScriptHelper FormViewScriptHelper => _formViewScriptHelper.Value;

    public GridViewScriptHelper GridViewScriptHelper => _gridViewScriptHelper.Value;

    public DataExportationScriptHelper DataExportationScriptHelper => _dataExportationScriptHelper.Value;

    public ScriptsHelper(
        Lazy<FormViewScriptHelper> formViewScriptHelper,
        Lazy<GridViewScriptHelper> gridViewScriptHelper,
        Lazy<DataExportationScriptHelper> dataExportationScriptHelper,
        Lazy<DataPanelScriptHelper> dataPanelScriptHelper)
    {
        _formViewScriptHelper = formViewScriptHelper;
        _gridViewScriptHelper = gridViewScriptHelper;
        _dataExportationScriptHelper = dataExportationScriptHelper;
        _dataPanelScriptHelper = dataPanelScriptHelper;
    }
}