using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationScripts
{
    private string Name { get; }
    private FormElement FormElement { get; }
    private IEncryptionService EncryptionService { get; }

    public DataExportationScripts(JJDataExportation dataExportation)
    {
        Name = dataExportation.Name;
        FormElement = dataExportation.FormElement;
        EncryptionService = dataExportation.EncryptionService;
    }
    
    public DataExportationScripts(string componentName,FormElement formElement, IEncryptionService encryptionService)
    {
        Name = componentName;
        FormElement = formElement;
        EncryptionService = encryptionService;
    }


    private string EncryptedRouteContext
    {
        get
        {
            var routeContext = RouteContext.FromFormElement(FormElement, ComponentContext.DataExportation);
            var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
            return encryptedRouteContext;
        }
    }

    public string GetStartExportationScript()
    {
        return $"DataExportationHelper.startExportation( '{Name}','{EncryptedRouteContext}');";
    }
    
    public string GetStopExportationScript(string stopMessage)
    {
        return $"DataExportationHelper.stopExportation('{Name}','{EncryptedRouteContext}','{stopMessage}');";
    }
    
    public string GetExportPopupScript()
    {
        return $"DataExportationHelper.openExportPopup('{Name}','{EncryptedRouteContext}');";
    }
}