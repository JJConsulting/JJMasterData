using System.Collections.Frozen;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using Microsoft.Extensions.DependencyInjection;
using NCalc;
using NCalc.Cache.Configuration;
using NCalc.DependencyInjection;

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

        builder.Services.AddNCalc().WithMemoryCache();
        builder.Services.AddScoped<IExpressionProvider,NCalcExpressionProvider>();
        
        builder.Services.PostConfigure<NCalcExpressionProviderOptions>(o =>
        {
            o.ReplaceDefaultExpressionProvider = options.ReplaceDefaultExpressionProvider;
            o.Context = options.Context;
        });
        
        return builder;
    }
    
    public static MasterDataServiceBuilder WithNCalcExpressions(this MasterDataServiceBuilder builder)
    {
        builder.WithNCalcExpressionProvider(new NCalcExpressionProviderOptions
        {
            ReplaceDefaultExpressionProvider = true,
            Context =
            {
                Functions = new Dictionary<string, ExpressionFunction>
                {
                    {"now", _ => DateTime.Now}
                }.ToFrozenDictionary()
            }
        });
        
        return builder;
    }
    
}