using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace JJMasterData.CommandLine.Hosting;

public sealed class TypeResolver(ServiceProvider serviceProvider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type)
    {
        return type == null ? null : serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        serviceProvider.Dispose();
    }
}
