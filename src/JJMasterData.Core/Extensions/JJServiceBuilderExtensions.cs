using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager.Exports.Abstractions;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Extensions;

public static class JJServiceBuilderExtensions
{
    public static JJServiceBuilder WithFormEventResolver(this JJServiceBuilder builder)
    {
        builder.Services.AddSingleton<IFormEventResolver,FormEventResolver>();

        return builder;
    }

    public static JJServiceBuilder WithPythonEngine<T>(this JJServiceBuilder builder) where T : IPythonEngine
    {
        builder.Services.AddSingleton(typeof(IPythonEngine), typeof(T));
        return builder;
    }
    
    public static JJServiceBuilder WithPdfExportation<T>(this JJServiceBuilder builder) where T : IPdfWriter
    {
        builder.Services.AddTransient(typeof(IPdfWriter), typeof(T));
        return builder;
    }

    public static JJServiceBuilder WithDictionaryRepository<T>(this JJServiceBuilder builder) where T : class, IDictionaryRepository 
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IDictionaryRepository, T>());
        return builder;
    }

    public static JJServiceBuilder WithExcelExportation<T>(this JJServiceBuilder builder) where T : class, IExcelWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IExcelWriter, T>());
        return builder;
    }

    public static JJServiceBuilder WithTextExportation<T>(this JJServiceBuilder builder) where T : class, ITextWriter
    {
        builder.Services.Replace(ServiceDescriptor.Transient<ITextWriter, T>());
        return builder;
    }
}