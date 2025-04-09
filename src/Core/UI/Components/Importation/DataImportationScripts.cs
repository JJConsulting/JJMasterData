#nullable enable

using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataImportationScripts(
    string name,
    FormElement formElement, 
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IEncryptionService encryptionService)
{
    private readonly string _modalTitle = formElement.Options.GridToolbarActions.ImportAction.Tooltip ?? string.Empty;

    private string GetEncryptedRouteContext()
    {
        var routeContext = RouteContext.FromFormElement(formElement, ComponentContext.DataImportation);
        var encryptedRouteContext = encryptionService.EncryptObject(routeContext);
        return encryptedRouteContext;
    }

    public DataImportationScripts(JJDataImportation dataImportation) : this(dataImportation.Name, dataImportation.FormElement, dataImportation.StringLocalizer, dataImportation.EncryptionService)
    {
    }
    
    public string GetStartProgressVerificationScript()
    {
        //language=Javascript
        return $"DataImportationHelper.startProgressVerification('{name}', '{GetEncryptedRouteContext()}')";
    }
    
    public string GetBackScript()
    {
        //language=Javascript
        return $"DataImportationHelper.back('{name}', '{GetEncryptedRouteContext()}')";
    }
    
    public string GetShowScript()
    {
        //language=Javascript
        return $"DataImportationHelper.show('{name}','{stringLocalizer[_modalTitle]}','{GetEncryptedRouteContext()}')";
    }

    
    public string GetHelpScript()
    {
        //language=Javascript
        return $"DataImportationHelper.help('{name}','{GetEncryptedRouteContext()}')";
    }
    
    public string GetStopScript(string stopMessage)
    {
        //language=Javascript
        return $"DataImportationHelper.stop('{name}','{GetEncryptedRouteContext()}','{stopMessage}')";
    }

    public string GetLogScript()
    {
        //language=Javascript
        return $"DataImportationHelper.showLog('{name}','{GetEncryptedRouteContext()}')";
    }
    
    public string GetUploadCallbackScript()
    {
        //language=Javascript
        return $"DataImportationHelper.uploadCallback('{name}','{GetEncryptedRouteContext()}')";
    }

    public static string GetCloseModalScript()
    {
        //language=Javascript
        return "DataImportationModal.getInstance().hide()";
    }
}