using System;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Actions;
using JJMasterData.Brasil.Models;
using JJMasterData.Brasil.Services;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JJMasterData.Brasil.Configuration;

public static class JJMasterDataServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithCepService<TService>(this JJMasterDataServiceBuilder builder) where TService : class, ICepService
    {
        builder.Services.AddScoped<ICepService, TService>();
        return builder;
    }

    
    public static JJMasterDataServiceBuilder WithReceitaFederalService<TService>(this JJMasterDataServiceBuilder builder) where TService : class, IReceitaFederalService
    {
        builder.Services.AddScoped<IReceitaFederalService, TService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithSintegra(this JJMasterDataServiceBuilder builder, Action<SintegraSettings>? configure = null)
    {
        builder.Services.AddOptions<SintegraSettings>().BindConfiguration("Sintegra");;

        if (configure is not null)
            builder.Services.PostConfigure(configure);
        
        builder.WithReceitaFederalService<SintegraService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithHubDev(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.Services.AddOptions<HubDevSettings>().BindConfiguration("HubDev");
        
        if(configure is not null)
            builder.Services.PostConfigure(configure);

        builder.WithReceitaFederalService<HubDevService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithViaCep(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddTransient<ICepService, ViaCepService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithCepActionPlugin<TCepService>(this JJMasterDataServiceBuilder builder) where TCepService : class, ICepService
    {
        builder.WithCepService<TCepService>();
        builder.WithActionPlugin<CepPluginActionHandler>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithCpfActionPlugin<TReceitaFederalService>(this JJMasterDataServiceBuilder builder) where TReceitaFederalService : class, IReceitaFederalService
    {
        builder.WithReceitaFederalService<TReceitaFederalService>();
        builder.WithActionPlugin<CpfPluginActionHandler>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithCnpjActionPlugin<TReceitaFederalService>(this JJMasterDataServiceBuilder builder) where TReceitaFederalService : class, IReceitaFederalService
    {
        builder.WithReceitaFederalService<TReceitaFederalService>();
        builder.WithActionPlugin<CnpjPluginActionHandler>();
        
        return builder;
    }

    
    public static JJMasterDataServiceBuilder WithHubDevCpfActionPlugin(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCpfActionPlugin<HubDevService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithHubDevCnpjActionPlugin(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCnpjActionPlugin<HubDevService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithHubDevCepActionPlugin(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCepActionPlugin<HubDevService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithBrasilActionPlugins(this JJMasterDataServiceBuilder builder)
    {
        builder.WithHubDevCnpjActionPlugin();
        builder.WithHubDevCpfActionPlugin();
        builder.WithHubDevCepActionPlugin();
        
        return builder;
    }
}