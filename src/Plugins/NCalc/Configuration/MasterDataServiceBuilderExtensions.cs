using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.NCalc.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithNCalcExpressionProvider(this MasterDataServiceBuilder builder, NCalcExpressionProviderOptions? options = null)
    {
        options ??= new NCalcExpressionProviderOptions();
        
        if (options.ReplaceDefaultExpressionProvider)
        {
            var defaultExpressionProvider = builder.Services.FirstOrDefault(descriptor => descriptor.ImplementationType == typeof(DefaultExpressionProvider));

            builder.Services.Remove(defaultExpressionProvider);
        }
        
        builder.Services.AddScoped<IExpressionProvider,NCalcExpressionProvider>();
        
        builder.Services.PostConfigure<NCalcExpressionProviderOptions>(o =>
        {
            o.ReplaceDefaultExpressionProvider = options.ReplaceDefaultExpressionProvider;
            o.EvaluateOptions = options.EvaluateOptions;
            o.AdditionalFunctions = options.AdditionalFunctions;
        });
        
        return builder;
    }
}