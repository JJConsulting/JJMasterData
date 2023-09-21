#nullable enable

using System;
using JJMasterData.Commons.Configuration;

using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Python.FormEvents;
using JJMasterData.Python.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Python.Extensions;
public static class Extensions
{

    public static JJMasterDataServiceBuilder WithPythonFormEventResolver(this JJMasterDataServiceBuilder builder, Action<PythonFormEventOptions>? configure = null)
    {
        if(configure != null)
            builder.Services.Configure(configure);
        
        builder.Services.AddTransient<IFormEventHandlerFactory,PythonFormEventHandlerFactory>();
        
        return builder;
    }
}