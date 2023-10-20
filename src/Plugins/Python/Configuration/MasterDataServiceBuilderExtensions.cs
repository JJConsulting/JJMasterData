#nullable enable

using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Python.FormEvents;
using JJMasterData.Python.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Python.Configuration;
public static class MasterDataServiceBuilderExtensions
{

    public static MasterDataServiceBuilder WithPythonFormEventResolver(this MasterDataServiceBuilder builder, Action<PythonFormEventOptions>? configure = null)
    {
        if(configure != null)
            builder.Services.Configure(configure);
        
        builder.Services.AddTransient<IFormEventHandlerFactory,PythonFormEventHandlerFactory>();
        
        return builder;
    }
}