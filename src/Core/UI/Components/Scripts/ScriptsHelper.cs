namespace JJMasterData.Core.Web.Components.Scripts;

public class ScriptsHelper
{
    public FormViewScriptHelper FormViewScriptHelper { get; }
    public GridViewScriptHelper GridViewScriptHelper { get; }
    public DataExportationScriptHelper DataExportationScriptHelper { get; }

    public ScriptsHelper(
        FormViewScriptHelper formViewScriptHelper,
        GridViewScriptHelper gridViewScriptHelper,
        DataExportationScriptHelper dataExportationScriptHelper
        )
    {
        FormViewScriptHelper = formViewScriptHelper;
        GridViewScriptHelper = gridViewScriptHelper;
        DataExportationScriptHelper = dataExportationScriptHelper;
    }
}