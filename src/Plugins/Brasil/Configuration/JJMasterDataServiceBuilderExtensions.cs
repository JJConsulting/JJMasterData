using System;
using JJMasterData.Brasil.Abstractions;
using JJMasterData.Brasil.Models;
using JJMasterData.Brasil.Services;
using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JJMasterData.Brasil.Extensions;

public static class JJMasterDataServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithSintegra(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddOptions<SintegraSettings>();
        builder.Services.AddTransient<IReceitaFederalService, SintegraService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithServicesHub(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddOptions<ServicesHubSettings>();
        builder.Services.AddTransient<IReceitaFederalService, ServicesHubService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithSintegra(this JJMasterDataServiceBuilder builder, Action<SintegraSettings> configure)
    {
        var settings = new SintegraSettings();

        configure(settings);
        
        builder.Services.AddOptions<SintegraSettings>(JsonConvert.SerializeObject(settings));

        builder.Services.AddTransient<IReceitaFederalService,SintegraService>();
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithServicesHub(this JJMasterDataServiceBuilder builder, Action<ServicesHubSettings> configure)
    {
        var settings = new ServicesHubSettings();

        configure(settings);
        
        builder.Services.AddOptions<ServicesHubSettings>(JsonConvert.SerializeObject(settings));
        
        builder.Services.AddTransient<IReceitaFederalService, ServicesHubService>();
        
        return builder;
    }
    
    public static JJMasterDataServiceBuilder WithViaCep(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddTransient<ICepService, ViaCepService>();
        
        return builder;
    }
}