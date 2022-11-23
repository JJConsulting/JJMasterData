using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.DI;
public class JJServiceBuilder
{
    public IServiceCollection Services { get; internal set; }

    public JJServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }
    
    public JJServiceBuilder AddDefaultServices()
    {
        Services.AddScoped<IEntityRepository, Factory>();
        return this;
    }

    public JJServiceBuilder WithBackgroundTask<T>() where T : IBackgroundTask
    {
        Services.AddSingleton(typeof(T));
        return this;
    }

    public JJServiceBuilder WithTranslator<T>() where T : ITranslator
    {
        Services.AddSingleton(typeof(T));
        return this;
    }

    public JJServiceBuilder WithJJMasterDataLogger()
    {
        Services.AddSingleton(typeof(Logger));
        return this;
    }
    
}