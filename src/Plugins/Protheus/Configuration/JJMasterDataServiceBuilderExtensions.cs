using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Protheus;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Protheus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Protheus.Configuration;

public static class JJMasterDataServiceBuilderExtensions
{
    public static JJMasterDataServiceBuilder WithProtheusExpressionProvider(this JJMasterDataServiceBuilder builder)
    {
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddScoped<IProtheusService, ProtheusService>();
        builder.Services.AddScoped<IExpressionProvider, ProtheusExpressionProvider>();

        return builder;
    }   
} 