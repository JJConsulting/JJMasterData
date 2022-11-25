using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Commons.DI;
public class JJServiceBuilder
{
    public IServiceCollection Services { get; }

    public JJServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }
    
    public JJServiceBuilder AddDefaultServices()
    {
        Services.AddScoped<IEntityRepository, Factory>();
        Services.AddLocalization();
        Services.AddTransient<ILocalizationProvider, JJMasterDataLocalizationProvider>();
        Services.AddTransient<IBackgroundTask, BackgroundTask>();
        return this;
    }

    public JJServiceBuilder WithBackgroundTask<T>() where T : class, IBackgroundTask
    {
        Services.Replace(ServiceDescriptor.Transient<IBackgroundTask, T>());
        return this;
    }

    public JJServiceBuilder WithTranslatorProvider<T>() where T : class, ILocalizationProvider
    {
        Services.Replace(ServiceDescriptor.Transient<ILocalizationProvider, T>());
        return this;
    }

    public JJServiceBuilder WithJJMasterDataLogger()
    {
        Services.AddSingleton(typeof(Logger));
        return this;
    }
    
}