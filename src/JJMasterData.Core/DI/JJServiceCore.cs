using System;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.DI;

public static class JJServiceCore 
{
   
    public static IDataDictionaryRepository DataDictionaryRepository
    {
        get
        {
            using var scope = JJService.Provider.CreateScope();
            return scope.ServiceProvider.GetService<IDataDictionaryRepository>();
        }
    }
    
    public static IFormEventResolver FormEventResolver
    {
        get
        {
            using var scope = JJService.Provider.CreateScope();
            return scope.ServiceProvider.GetService<IFormEventResolver>();
        }
    }
    
}