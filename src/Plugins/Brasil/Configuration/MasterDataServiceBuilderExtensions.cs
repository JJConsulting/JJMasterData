using System;
using System.Net.Http;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Actions;
using JJMasterData.Brasil.Services;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Brasil.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithCepService<TService>(this MasterDataServiceBuilder builder) where TService : class, ICepService
    {
        builder.Services.AddScoped<ICepService, TService>();
        return builder;
    }

    
    public static MasterDataServiceBuilder WithReceitaFederalService<TService>(this MasterDataServiceBuilder builder) where TService : class, IReceitaFederalService
    {
        builder.Services.AddScoped<IReceitaFederalService, TService>();
        return builder;
    }
    
    public static MasterDataServiceBuilder WithSintegra(this MasterDataServiceBuilder builder, Action<SintegraSettings>? configure = null)
    {
        builder.Services.AddOptions<SintegraSettings>().BindConfiguration("Sintegra");;

        if (configure is not null)
            builder.Services.PostConfigure(configure);
        
        builder.WithReceitaFederalService<SintegraService>();
        return builder;
    }
    
    public static MasterDataServiceBuilder WithHubDev(this MasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.Services.AddOptions<HubDevSettings>().BindConfiguration("HubDev");
        
        if(configure is not null)
            builder.Services.PostConfigure(configure);

        builder.WithReceitaFederalService<HubDevService>();
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithViaCep(this MasterDataServiceBuilder builder)
    {
        builder.Services.AddTransient<ICepService, ViaCepService>();
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithCepActionPlugin<TCepService>(this MasterDataServiceBuilder builder) where TCepService : class, ICepService
    {
        builder.WithCepService<TCepService>();
        builder.WithActionPlugin<CepPluginActionHandler>();
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithCpfActionPlugin<TReceitaFederalService>(this MasterDataServiceBuilder builder) where TReceitaFederalService : class, IReceitaFederalService
    {
        builder.WithReceitaFederalService<TReceitaFederalService>();
        builder.WithActionPlugin<CpfPluginActionHandler>();
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithCnpjActionPlugin<TReceitaFederalService>(this MasterDataServiceBuilder builder) where TReceitaFederalService : class, IReceitaFederalService
    {
        builder.WithReceitaFederalService<TReceitaFederalService>();
        builder.WithActionPlugin<CnpjPluginActionHandler>();
        
        return builder;
    }

    
    public static MasterDataServiceBuilder WithHubDevCpfActionPlugin(this MasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCpfActionPlugin<HubDevService>();
        return builder;
    }
    
    public static MasterDataServiceBuilder WithHubDevCnpjActionPlugin(this MasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCnpjActionPlugin<HubDevService>();
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithHubDevCepActionPlugin(this MasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCepActionPlugin<HubDevService>();
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithBrasilActionPlugins(this MasterDataServiceBuilder builder)
    {
        builder.Services.TryAddSingleton<HttpClient>();
        builder.WithHubDevCnpjActionPlugin();
        builder.WithHubDevCpfActionPlugin();
        builder.WithHubDevCepActionPlugin();
        
        return builder;
    }
}