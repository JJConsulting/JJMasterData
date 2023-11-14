#nullable enable

using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Routing;

namespace JJMasterData.Core.UI.Components;

internal class DataImportationScripts(string name, FormElement formElement, IEncryptionService encryptionService)
{
    public string Name { get; } = name;
    public FormElement FormElement { get; } = formElement;
    public IEncryptionService EncryptionService { get; } = encryptionService;

    private string GetEncryptedRouteContext(ComponentContext context = ComponentContext.DataImportation)
    {
        var routeContext = RouteContext.FromFormElement(FormElement, context);
        var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
        return encryptedRouteContext;
    }

    public DataImportationScripts(JJDataImportation dataImportation) : this(dataImportation.Name, dataImportation.FormElement, dataImportation.EncryptionService)
    {
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