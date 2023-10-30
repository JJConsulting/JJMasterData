#nullable enable

using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Python.Configuration.Options;
using JJMasterData.Python.Engine;
using JJMasterData.Python.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;

namespace JJMasterData.Python.Configuration;
public static class MasterDataServiceBuilderExtensions
{
    public static MasterDataServiceBuilder WithPythonEvents(this MasterDataServiceBuilder builder, Action<PythonEngineOptions>? configure = null)
    {
        builder.Services.AddOptions<PythonEngineOptions>().BindConfiguration("JJMasterData:Python");
        
        if(configure != null)
            builder.Services.PostConfigure(configure);
    
        builder.Services.AddScoped<IFormEventHandlerFactory,PythonEventHandlerFactory>();
        builder.Services.AddScoped<IGridEventHandlerFactory,PythonEventHandlerFactory>();

        builder.Services.AddSingleton<ScriptEngine>(svp =>
        {
            var additionalPaths = svp.GetRequiredService<IOptions<PythonEngineOptions>>().Value.AdditionalScriptsPaths;
            return PythonEngineFactory.CreateScriptEngine(additionalPaths);
        });
        
        return builder;
    }
}