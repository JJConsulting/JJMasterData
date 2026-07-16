using Microsoft.Extensions.DependencyInjection;

namespace JJConsulting.MasterData.Abstractions;

public interface IMasterDataServiceBuilder
{
    public IServiceCollection Services { get; }
}