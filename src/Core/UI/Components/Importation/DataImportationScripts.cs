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
    
    private string EncryptedRouteContext
    {
        get
        {
            var routeContext = RouteContext.FromFormElement(FormElement, ComponentContext.DataImportation);
            var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
            return encryptedRouteContext;
        }
    }
    
    public DataImportationScripts(JJDataImportation dataImportation)
    {
        Name = dataImportation.Name;
        FormElement = dataImportation.FormElement;
        EncryptionService = dataImportation.EncryptionService;
    }

    public string GetStartImportationScript()
    {
        return $"DataImportation.startImportation('{Name}','{EncryptedRouteContext}')";
    }
    
    public string GetStopImportationScript(string stopMessage)
    {
        return $"DataImportation.stopImportation('{Name}','{EncryptedRouteContext}','{stopMessage}')";
    }
}