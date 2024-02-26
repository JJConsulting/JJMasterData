using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationScripts(string componentName,FormElement formElement, IEncryptionService encryptionService)
{
    private string Name { get; } = componentName;
    private FormElement FormElement { get; } = formElement;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    public DataExportationScripts(JJDataExportation dataExportation) : this(dataExportation.Name, dataExportation.FormElement, dataExportation.EncryptionService)
    {
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
    
    
    public string GetStartProgressVerificationScript()
    {
        //language=Javascript
        return $"DataExportationHelper.startProgressVerification( '{Name}','{EncryptedRouteContext}');";
    }


    public string GetStartExportationScript()
    {
        //language=Javascript
        return $"DataExportationHelper.startExportation( '{Name}','{EncryptedRouteContext}');";
    }
    
    public string GetStopExportationScript(string stopMessage)
    {
        //language=Javascript
        return $"DataExportationHelper.stopExportation('{Name}','{EncryptedRouteContext}','{stopMessage}');";
    }
    
    public string GetExportModalScript()
    {
        //language=Javascript
        return $"DataExportationHelper.openExportPopup('{Name}','{EncryptedRouteContext}');";
    }
}