using System;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Commons.Storage;

public static class MasterDataServiceCollectionExtensions
{
    extension(MasterDataServiceBuilder builder)
    {
        public MasterDataServiceBuilder WithFileStorage(Func<IServiceProvider, IFileStorage> implementationFactory)
        {
            builder.Services.Replace(ServiceDescriptor.Singleton(implementationFactory));
            return builder;
        }
    
        public MasterDataServiceBuilder WithFileStorage<T>() where T : IFileStorage
        {
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IFileStorage),typeof(T)));
            return builder;
        }
    }
}