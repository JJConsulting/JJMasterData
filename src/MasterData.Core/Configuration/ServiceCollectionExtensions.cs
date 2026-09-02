using System.Linq;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Exportation;
using JJMasterData.Core.DataManager.Exportation.Abstractions;
using JJMasterData.Core.DataManager.Exportation.Background;
using JJMasterData.Core.DataManager.Exportation.Formats;
using JJMasterData.Core.DataManager.Importation;
using JJMasterData.Core.DataManager.Importation.Abstractions;
using JJMasterData.Core.DataManager.Importation.Background;
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
        
            services.AddScoped<CsvExportFormat>();
            services.AddScoped<IExportFormatRegistration, ExportFormatRegistration<CsvExportFormat, DelimitedTextExportOptions>>();
            services.AddScoped<TextExportFormat>();
            services.AddScoped<IExportFormatRegistration, ExportFormatRegistration<TextExportFormat, DelimitedTextExportOptions>>();
            services.AddScoped<ExcelXlsExportFormat>();
            services.AddScoped<IExportFormatRegistration, ExportFormatRegistration<ExcelXlsExportFormat, ExcelXlsExportOptions>>();
            services.AddScoped<ExcelXlsxExportFormat>();
            services.AddScoped<IExportFormatRegistration, ExportFormatRegistration<ExcelXlsxExportFormat, ExcelXlsxExportOptions>>();
            services.AddScoped(provider => new ExportFormatCatalog(provider.GetServices<IExportFormatRegistration>()));
            services.AddScoped<ExportJobService>();
            services.AddScoped<BackgroundJobHandler<ExportRequest>, ExportJobHandler>();
            services.AddScoped<CsvImportReader>();
            services.AddScoped<IImportReaderRegistration, ImportReaderRegistration<CsvImportReader, CsvImportOptions>>();
            services.AddScoped(provider => new ImportFormatCatalog(provider.GetServices<IImportReaderRegistration>()));
            services.AddScoped<ImportJobService>();
            services.AddScoped<BackgroundJobHandler<ImportRequest>, ImportJobHandler>();
            services.AddFactories();
        }
    }
}
