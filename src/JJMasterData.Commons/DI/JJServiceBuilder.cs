using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Options;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.DI;
public class JJServiceBuilder
{
    public IServiceCollection Services { get; }

    public JJServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }
    
    public JJServiceBuilder AddDefaultServices(IConfiguration configuration)
    {
        Services.AddOptions<JJMasterDataOptions>().Bind(configuration.GetSection("JJMasterData"));
        Services.AddLocalization();
        Services.AddLogging(builder =>
        {
            builder.AddJJMasterDataLogger();
        });
        Services.AddScoped<IEntityRepository,Factory>();
        Services.AddTransient<ILocalizationProvider, JJMasterDataLocalizationProvider>();
        Services.AddTransient<IBackgroundTask, BackgroundTask>();
        return this;
    }

    public JJServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTask
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTask, T>());
        return this;
    }

    public JJServiceBuilder WithLocalizationProvider<T>() where T : class, ILocalizationProvider
    {
        Services.Replace(ServiceDescriptor.Transient<ILocalizationProvider, T>());
        return this;
    }
}
