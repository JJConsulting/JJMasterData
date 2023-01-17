using System;
using JJMasterData.Commons.DI;
using JJMasterData.ReceitaFederal.Abstractions;
using JJMasterData.ReceitaFederal.Models;
using JJMasterData.ReceitaFederal.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JJMasterData.ReceitaFederal.Extensions;

public static class Extensions
{
    public static JJServiceBuilder WithSintegra(this JJServiceBuilder builder)
    {
        builder.Services.AddOptions<SintegraSettings>();
        builder.Services.AddTransient<IReceitaFederal, SintegraService>();
        return builder;
    }
    
    public static JJServiceBuilder WithServicesHub(this JJServiceBuilder builder)
    {
        builder.Services.AddOptions<ServicesHubSettings>();
        builder.Services.AddTransient<IReceitaFederal, ServicesHubService>();
        return builder;
    }
    
    public static JJServiceBuilder WithSintegra(this JJServiceBuilder builder, Action<SintegraSettings> configure)
    {
        var settings = new SintegraSettings();

        configure(settings);
        
        builder.Services.AddOptions<SintegraSettings>(JsonConvert.SerializeObject(settings));

        builder.Services.AddTransient<IReceitaFederal,SintegraService>();
        return builder;
    }
    
    public static JJServiceBuilder WithServicesHub(this JJServiceBuilder builder, Action<ServicesHubSettings> configure)
    {
        var settings = new ServicesHubSettings();

        configure(settings);
        
        builder.Services.AddOptions<ServicesHubSettings>(JsonConvert.SerializeObject(settings));
        
        builder.Services.AddTransient<IReceitaFederal, ServicesHubService>();
        
        return builder;
    }
}