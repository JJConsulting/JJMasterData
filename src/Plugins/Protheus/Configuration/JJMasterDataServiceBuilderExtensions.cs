using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Protheus.Configuration;

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