using JJMasterData.Commons.Background;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Background;
using JJMasterData.Core.DataManager.Exportation.Formats;
using JJMasterData.Core.DataManager.Importation.Background;
using JJMasterData.Core.Html.Templates;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public MasterDataServiceBuilder AddJJMasterDataCore()
        {
            services.AddMasterDataCoreServices();

            return services.AddJJMasterDataCommons();
        }

        public MasterDataServiceBuilder AddJJMasterDataCore(MasterDataCoreOptionsConfiguration optionsConfiguration
        )
        {
            if (optionsConfiguration.ConfigureCore != null) 
                services.PostConfigure(optionsConfiguration.ConfigureCore);

            services.AddMasterDataCoreServices();
            return services.AddJJMasterDataCommons(optionsConfiguration.ConfigureCommons);
        }

        public MasterDataServiceBuilder AddJJMasterDataCore(IConfiguration configuration)
        {
            services.Configure<MasterDataCoreOptions>(configuration.GetJJMasterData());

            services.AddMasterDataCoreServices();

            return services.AddJJMasterDataCommons(configuration);
        }

        private void AddMasterDataCoreServices()
        {
            services.AddOptions<MasterDataCoreOptions>().BindConfiguration("JJMasterData");

            services.AddHttpServices();
            services.AddDataProtection();
            services.TryAddScoped<IGridPreferenceStore>(provider =>
            {
                var context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                if (context?.Features.Get<ISessionFeature>()?.Session != null)
                    return ActivatorUtilities.CreateInstance<SessionGridPreferenceStore>(provider);
                
                return ActivatorUtilities.CreateInstance<CookieGridPreferenceStore>(provider);
            });
            services.AddDataDictionaryServices();
            services.AddDataManagerServices();
            services.AddEventHandlers();
            services.AddExpressionServices();
            services.AddActionServices();

            services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();

            services.AddTransient<HtmlTemplateRenderer>();
        
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IExportFormat, CsvExportFormat>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IExportFormat, TextExportFormat>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IExportFormat, ExcelXlsExportFormat>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IExportFormat, ExcelXlsxExportFormat>());
            
            services.AddScoped<ExportFormatCatalog>();
            services.AddScoped<ExportJobService>();
            services.AddScoped<BackgroundJobHandler<ExportRequest>, ExportJobHandler>();
         
            services.AddScoped<ImportJobService>();
            services.AddScoped<BackgroundJobHandler<ImportRequest>, ImportJobHandler>();
            services.AddFactories();
        }
    }
}
