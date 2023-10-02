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
    public static JJMasterDataServiceBuilder WithReceitaFederalService<TService>(this JJMasterDataServiceBuilder builder) where TService : class, IReceitaFederalService
    {
        builder.Services.AddTransient<IReceitaFederalService, TService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithSintegra(this JJMasterDataServiceBuilder builder, Action<SintegraSettings>? configure = null)
    {
        builder.Services.AddOptions<SintegraSettings>().BindConfiguration("Sintegra");;

        if (configure is not null)
            builder.Services.PostConfigure(configure);
        
        builder.Services.AddTransient<IReceitaFederalService, SintegraService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithHubDev(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.Services.AddOptions<HubDevSettings>().BindConfiguration("HubDev");
        
        if(configure is not null)
            builder.Services.PostConfigure(configure);
        
        builder.Services.AddTransient<IReceitaFederalService, HubDevService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithViaCep(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddTransient<ICepService, ViaCepService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithCepPluginAction(this JJMasterDataServiceBuilder builder)
    {
        builder.WithViaCep();
        builder.WithActionPlugin<CepPluginActionHandler>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithCpfPluginAction<TReceitaFederalService>(this JJMasterDataServiceBuilder builder) where TReceitaFederalService : class, IReceitaFederalService
    {
        builder.WithReceitaFederalService<TReceitaFederalService>();
        builder.WithActionPlugin<CpfPluginActionHandler>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithCnpjPluginAction<TReceitaFederalService>(this JJMasterDataServiceBuilder builder) where TReceitaFederalService : class, IReceitaFederalService
    {
        builder.WithReceitaFederalService<TReceitaFederalService>();
        builder.WithActionPlugin<CnpjPluginActionHandler>();
        
        return builder;
    }

    
    public static JJMasterDataServiceBuilder WithHubDevCpfPluginAction(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCpfPluginAction<HubDevService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithHubDevCnpjPluginAction(this JJMasterDataServiceBuilder builder, Action<HubDevSettings>? configure = null)
    {
        builder.WithHubDev(configure);
        builder.WithCnpjPluginAction<HubDevService>();
        
        return builder;
    }
}