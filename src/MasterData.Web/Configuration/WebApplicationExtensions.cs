using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Configuration.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Web.Configuration;

public static partial class WebApplicationExtensions
{
    [PublicAPI]
    public static async Task UseMasterDataSeedingAsync(this WebApplication app)
    {
        using var dbScope = app.Services.CreateScope();
        var serviceProvider = dbScope.ServiceProvider;
    
        var dataDictionaryRepository = serviceProvider.GetRequiredService<IDataDictionaryRepository>();
        var auditLogService = serviceProvider.GetRequiredService<AuditLogService>();

        await auditLogService.CreateStructureIfNotExistsAsync();
        await dataDictionaryRepository.CreateStructureIfNotExistsAsync();
    }
    
    public static ControllerActionEndpointConventionBuilder MapDataDictionary(this WebApplication app)
    {
        return app.MapAreaControllerRoute(
            name: "dataDictionary",
            areaName: "DataDictionary",
            pattern: "DataDictionary/{controller=Element}/{action=Index}/{elementName?}/{fieldName?}");
    }
    
    public static ControllerActionEndpointConventionBuilder MapMasterData(this WebApplication app)
    {
        return app.MapAreaControllerRoute(
            name: "masterData",
            areaName: "MasterData",
            pattern:  "MasterData/{controller}/{action}/{elementName?}/{fieldName?}/{id?}");
    }
}