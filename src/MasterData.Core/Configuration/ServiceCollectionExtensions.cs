using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.Html.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddDataDictionaryServices();
            services.AddDataManagerServices();
            services.AddEventHandlers();
            services.AddExpressionServices();
            services.AddActionServices();

            services.AddScoped<IDataDictionaryRepository, SqlDataDictionaryRepository>();

            services.AddTransient<HtmlTemplateRenderer>();
        
            services.AddScoped<IExcelWriter, ExcelWriter>();
            services.AddScoped<ITextWriter, TextWriter>();

            services.AddFactories();
        }
    }
}
