using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJConsulting.MasterData.Abstractions;

public static class MasterDataServiceCollectionExtensions
{
    extension(IMasterDataServiceBuilder builder)
    {
        public IMasterDataServiceBuilder WithFileStorage(Func<IServiceProvider, IFileStorage> implementationFactory)
        {
            builder.Services.Replace(ServiceDescriptor.Singleton(implementationFactory));
            return builder;
        }
    
        public IMasterDataServiceBuilder WithFileStorage<T>() where T : IFileStorage
        {
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IFileStorage),typeof(T)));
            return builder;
        }
    }
}