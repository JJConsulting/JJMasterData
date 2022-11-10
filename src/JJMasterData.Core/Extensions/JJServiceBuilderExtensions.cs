using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder WithFormEvents(this JJServiceBuilder builder)
    {
        var assemblies = new List<Assembly>
        {
            Assembly.GetCallingAssembly()
        };

        return builder.WithFormEvents(assemblies.ToArray());
    }

    public static JJServiceBuilder WithFormEvents(this JJServiceBuilder builder, params Assembly[] assemblies)
    {
        FormEventManager.Assemblies = assemblies;

        var formEventTypes = FormEventManager.GetFormEventTypes(assemblies);
        foreach (var type in formEventTypes.Where(type => !type.IsAbstract))
        {
            builder.Services.Add(new ServiceDescriptor(type, type, ServiceLifetime.Transient));
        }
        return builder;
    }

    public static JJServiceBuilder WithPythonEngine(this JJServiceBuilder builder, IPythonEngine engine)
    {
        builder.Services.AddSingleton(engine);
        return builder;
    }

    public static JJServiceBuilder WithPdfExportation<T>(this JJServiceBuilder builder) where T : IPdfWriter
    {
        builder.Services.AddTransient(typeof(IPdfWriter), typeof(T));
        return builder;
    }

    public static JJServiceBuilder WithExcelExportation<T>(this JJServiceBuilder builder) where T : IExcelWriter
    {
        builder.Services.AddTransient(typeof(IExcelWriter), typeof(T));
        return builder;
    }

    public static JJServiceBuilder WithTextExportation<T>(this JJServiceBuilder builder) where T : ITextWriter
    {
        builder.Services.AddTransient(typeof(ITextWriter), typeof(T));
        return builder;
    }
}