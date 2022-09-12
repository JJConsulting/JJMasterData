using System;
using System.Linq;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Settings;
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
        Services.AddSingleton<ISettings, JJMasterDataSettings>();
        Services.AddScoped<IDataAccess, DataAccess>();
        
        return this;
    }

    private void DeleteServiceIfExists<T>()
    {
        var service = Services.First(s => s.ImplementationType == typeof(T));

        Services.Remove(service);
    }

    public JJServiceBuilder WithSettings<T>() where T : ISettings
    {
        DeleteServiceIfExists<T>();

        Services.AddSingleton(typeof(T));

        return this;
    }

    public JJServiceBuilder WithSettings(ISettings settings)
    {
        DeleteServiceIfExists<ISettings>();

        Services.AddSingleton(settings);

        return this;
    }

    public JJServiceBuilder WithSettings(Action<ISettings> configure)
    {
        DeleteServiceIfExists<ISettings>();

        var settings = new JJMasterDataSettings();

        configure(settings);

        Services.AddSingleton(settings);

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
    
    public JJServiceBuilder WithDataAccess<T>() where T : IDataAccess
    {
        DeleteServiceIfExists<IDataAccess>();
        
        Services.AddScoped(typeof(IDataAccess),typeof(T));

        return this;
    }
}