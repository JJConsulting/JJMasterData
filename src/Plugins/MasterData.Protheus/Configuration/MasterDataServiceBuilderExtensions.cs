using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Protheus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Protheus.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithProtheusServices(this MasterDataServiceBuilder builder)
    {
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddProtheusService();
        builder.Services.AddScoped<IExpressionProvider, ProtheusExpressionProvider>();
        
        return builder;
    }   
} 