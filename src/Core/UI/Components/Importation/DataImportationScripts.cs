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
    private string Name { get; } = name;
    private string ModalTitle { get; } = formElement.Options.GridToolbarActions.ImportAction.Tooltip;
    private FormElement FormElement { get; } = formElement;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    private string GetEncryptedRouteContext()
    {
        var routeContext = RouteContext.FromFormElement(FormElement, ComponentContext.DataImportation);
        var encryptedRouteContext = EncryptionService.EncryptObject(routeContext);
        return encryptedRouteContext;
    }

    public DataImportationScripts(JJDataImportation dataImportation) : this(dataImportation.Name, dataImportation.FormElement, dataImportation.StringLocalizer, dataImportation.EncryptionService)
    {
    }
    
    public string GetStartProgressVerificationScript()
    {
        //language=Javascript
        return $"DataImportationHelper.startProgressVerification('{Name}', '{GetEncryptedRouteContext()}')";
    }
    
    public string GetBackScript()
    {
        //language=Javascript
        return $"DataImportationHelper.back('{Name}', '{GetEncryptedRouteContext()}')";
    }
    
    public string GetShowScript()
    {
        //language=Javascript
        return $"DataImportationHelper.show('{Name}','{StringLocalizer[ModalTitle]}','{GetEncryptedRouteContext()}')";
    }

    
    public string GetHelpScript()
    {
        //language=Javascript
        return $"DataImportationHelper.help('{Name}','{GetEncryptedRouteContext()}')";
    }
    
    public string GetStopScript(string stopMessage)
    {
        //language=Javascript
        return $"DataImportationHelper.stop('{Name}','{GetEncryptedRouteContext()}','{stopMessage}')";
    }

    public string GetLogScript()
    {
        //language=Javascript
        return $"DataImportationHelper.showLog('{Name}','{GetEncryptedRouteContext()}')";
    }
    
    public string GetUploadCallbackScript()
    {
        //language=Javascript
        return $"DataImportationHelper.uploadCallback('{Name}','{GetEncryptedRouteContext()}')";
    }

    public static string GetCloseModalScript()
    {
        //language=Javascript
        return "DataImportationModal.getInstance().hide()";
    }
}