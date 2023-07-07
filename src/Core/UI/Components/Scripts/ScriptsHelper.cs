namespace JJMasterData.Core.Web.Components.Scripts;

public class ScriptsHelper
{
    public DataPanelScriptHelper DataPanelScriptHelper { get; }
    public FormViewScriptHelper FormViewScriptHelper { get; }
    public GridViewScriptHelper GridViewScriptHelper { get; }
    public DataExportationScriptHelper DataExportationScriptHelper { get; }

    public ScriptsHelper(
        FormViewScriptHelper formViewScriptHelper,
        GridViewScriptHelper gridViewScriptHelper,
        DataExportationScriptHelper dataExportationScriptHelper,
        DataPanelScriptHelper dataPanelScriptHelper)
    {
        FormViewScriptHelper = formViewScriptHelper;
        GridViewScriptHelper = gridViewScriptHelper;
        DataExportationScriptHelper = dataExportationScriptHelper;
        DataPanelScriptHelper = dataPanelScriptHelper;
    }
}