using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Configuration;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        [PublicAPI]
        public async Task UseMasterDataSeedingAsync()
        {
            using var dbScope = app.Services.CreateScope();
            var serviceProvider = dbScope.ServiceProvider;
    
            var dataDictionaryService = serviceProvider.GetRequiredService<DataDictionaryService>();
            var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();

            await auditLogService.CreateStructureIfNotExistsAsync();
            await dataDictionaryService.CreateStructureIfNotExistsAsync();
        }

        public ControllerActionEndpointConventionBuilder MapDataDictionary()
        {
            return app.MapAreaControllerRoute(
                name: "dataDictionary",
                areaName: "DataDictionary",
                pattern: "DataDictionary/{controller=Element}/{action=Index}/{elementName?}/{fieldName?}");
        }

        public ControllerActionEndpointConventionBuilder MapMasterData()
        {
            return app.MapAreaControllerRoute(
                name: "masterData",
                areaName: "MasterData",
                pattern:  "MasterData/{controller}/{action}/{elementName?}/{fieldName?}/{id?}");
        }
    }
}