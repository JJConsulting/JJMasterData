using JJMasterData.Commons.Configuration;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Protheus.Configuration;
using JJMasterData.Web.Configuration.Options;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.RecursiveProcedureAction.Configuration;

public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithRecursiveProcedureAction(this MasterDataServiceBuilder builder)
    {
        builder.Services.AddSingleton<HttpClient>();
        builder.WithProtheusServices();
        builder.Services.AddScoped<IPluginHandler, RecursiveProcedurePluginActionHandler>();
        builder.Services.PostConfigure<MasterDataWebOptions>(options =>
        {
            options.CustomScriptsPaths.Add("/_content/JJMasterData.RecursiveProcedureAction/js/executionSequence.js");
        });
        return builder;
    }   
}