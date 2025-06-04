using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class DataExportationScripts(
    string componentName,
    FormElement formElement,
    IEncryptionService encryptionService)
{
    public DataExportationScripts(JJDataExportation dataExportation) : this(dataExportation.Name,
        dataExportation.FormElement, dataExportation.EncryptionService)
    {
    }

    private string EncryptedRouteContext
    {
        get
        {
            var routeContext = RouteContext.FromFormElement(formElement, ComponentContext.DataExportation);
            var encryptedRouteContext = encryptionService.EncryptObject(routeContext);
            return encryptedRouteContext;
        }
    }


    public string GetStartProgressVerificationScript()
    {
        //language=Javascript
        return $"DataExportationHelper.startProgressVerification( '{componentName}','{EncryptedRouteContext}');";
    }


    public string GetStartExportationScript()
    {
        //language=Javascript
        return $"DataExportationHelper.startExportation( '{componentName}','{EncryptedRouteContext}');";
    }

    public string GetStopExportationScript(string stopMessage)
    {
        //language=Javascript
        return $"DataExportationHelper.stopExportation('{componentName}','{EncryptedRouteContext}','{stopMessage}');";
    }

    public string GetExportModalScript()
    {
        //language=Javascript
        return $"DataExportationHelper.openExportPopup('{componentName}','{EncryptedRouteContext}');";
    }
}