#nullable enable

using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.UI.Components.Importation;

internal class DataImportationScripts
{
    public string Name { get; }
    public FormElement FormElement { get; }
    public IEncryptionService EncryptionService { get; }

    private string GetEncryptedRouteContext(ComponentContext context = ComponentContext.DataImportation)
    {
        var routeContext = RouteContext.FromFormElement(FormElement, context);
        var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
        return encryptedRouteContext;
    }

    public DataImportationScripts(string name, FormElement formElement, IEncryptionService encryptionService)
    {
        Name = name;
        FormElement = formElement;
        EncryptionService = encryptionService;
    }
    
    public DataImportationScripts(JJDataImportation dataImportation)
    {
        Name = dataImportation.Name;
        FormElement = dataImportation.FormElement;
        EncryptionService = dataImportation.EncryptionService;
    }
    
    public string GetShowScript()
    {
        return $"DataImportationHelper.show('{Name}','{GetEncryptedRouteContext()}', '{GetEncryptedRouteContext(ComponentContext.GridViewReload)}')";
    }

    
    public string GetHelpScript()
    {
        return $"DataImportationHelper.help('{Name}','{GetEncryptedRouteContext()}')";
    }
    
    public string GetStopScript(string stopMessage)
    {
        return $"DataImportationHelper.stop('{Name}','{GetEncryptedRouteContext()}','{stopMessage}')";
    }

    public string GetLogScript()
    {
        return $"DataImportationHelper.showLog('{Name}','{GetEncryptedRouteContext()}')";
    }
}