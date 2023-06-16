using JJMasterData.BlazorClient.Services;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.BlazorClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJJMasterDataBlazorClient(this IServiceCollection services, RenderMode renderMode = RenderMode.ServerPrerendered)
    {
        if (renderMode is RenderMode.ServerPrerendered or RenderMode.Server)
        {
            services.AddServerSideBlazor(options =>
            {
#if DEBUG
                options.DetailedErrors = true;
#endif
            });
            
            services.AddTransient<IExpressionsService, ExpressionsService>();
            services.AddTransient<ISearchBoxService, SearchBoxService>();
            services.AddTransient<IEntityService, EntityService>();
            services.AddTransient<IDataDictionaryService, DataDictionaryService>();
            services.AddTransient<IFieldsService, FieldsService>();
            services.AddTransient<IFieldValidationService, FieldValidationService>();
            services.AddTransient<IFieldFormattingService, FieldFormattingService>();
        }
        else
        {
            throw new NotSupportedException("This RenderMode is not supported by JJMasterData by now.");
        }

  
        return services;
    }
}