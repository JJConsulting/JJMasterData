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
            o.ExpressionOptions = options.ExpressionOptions;
            o.AdditionalFunctions = options.AdditionalFunctions;
        });
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithNCalcExpression(this MasterDataServiceBuilder builder)
    {
        builder.WithNCalcExpressionProvider(new NCalcExpressionProviderOptions
        {
            ReplaceDefaultExpressionProvider = true,
            AdditionalFunctions =
            [
                (name, args) =>
                {
                    if (name == "now")
                        args.Result = DateTime.Now;
                }
            ]
        });
        
        return builder;
    }
    
}