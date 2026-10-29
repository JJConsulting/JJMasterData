using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Services;

/// <summary>Provides access to data dictionaries and manages their in-memory cache.</summary>
public class DataDictionaryService(
    IDataDictionaryRepository repository,
    IMemoryCache memoryCache,
    IOptionsSnapshot<MasterDataCoreOptions> options)
{
    private static readonly object ElementNameListCacheKey = new();
    private readonly bool _enableCaching = options.Value.EnableDataDictionaryCaching;

    private static object ElementCacheKey(string name)
    {
        return (typeof(DataDictionaryService), name);
    }

    public FormElement? GetFormElement(string elementName)
    {
        if (!_enableCaching)
            return repository.GetFormElement(elementName);

        var key = ElementCacheKey(elementName);
        if (memoryCache.TryGetValue(key, out FormElement? cached))
            return cached!.DeepCopy();

        var element = repository.GetFormElement(elementName);
        
        if (element is not null && _enableCaching)
            memoryCache.Set(key, element);

        return element;
    }

    public async ValueTask<FormElement> GetFormElementAsync(string elementName)
    {
        if (!_enableCaching)
            return await repository.GetFormElementAsync(elementName);

        var key = ElementCacheKey(elementName);
        if (memoryCache.TryGetValue(key, out FormElement? cached))
            return cached!.DeepCopy();

        var element = await repository.GetFormElementAsync(elementName);
        
        if(_enableCaching)
            memoryCache.Set(key, element);
        
        return element;
    }

    public async ValueTask<List<string>> GetElementNameListAsync()
    {
        if (!_enableCaching)
            return new List<string>(await repository.GetElementNameListAsync());

        if (memoryCache.TryGetValue(ElementNameListCacheKey, out List<string>? cached))
            return new List<string>(cached!);

        var names = await repository.GetElementNameListAsync();
        
        if(_enableCaching)
            memoryCache.Set(ElementNameListCacheKey, new List<string>(names));
        
        return names;
    }

    public List<FormElement> GetFormElementList(bool? apiSync = null)
    {
        return repository.GetFormElementList(apiSync);
    }

    public Task<List<FormElement>> GetFormElementListAsync(bool? apiSync = null)
    {
        return repository.GetFormElementListAsync(apiSync);
    }

    public Task<ListResult<FormElementInfo>> GetFormElementInfoListAsync(DataDictionaryFilter filters,
        OrderByData? orderByData, int recordsPerPage, int currentPage)
    {
        return repository.GetFormElementInfoListAsync(filters, orderByData, recordsPerPage, currentPage);
    }

    public Task<bool> ExistsAsync(string elementName)
    {
        return repository.ExistsAsync(elementName);
    }

    public Task CreateStructureIfNotExistsAsync()
    {
        return repository.CreateStructureIfNotExistsAsync();
    }

    public void InsertOrReplace(FormElement formElement)
    {
        repository.InsertOrReplace(formElement);
        ClearCache(formElement);
    }

    public async Task InsertOrReplaceAsync(FormElement formElement)
    {
        await repository.InsertOrReplaceAsync(formElement);
        ClearCache(formElement);
    }

    public async Task InsertOrReplaceAsync(IEnumerable<FormElement> formElements)
    {
        var elements = formElements.ToList();
        try
        {
            await repository.InsertOrReplaceAsync(elements);
        }
        finally
        {
            foreach (var element in elements)
                ClearCache(element);
        }
    }

    public async Task DeleteAsync(string elementName)
    {
        await repository.DeleteAsync(elementName);

        if (_enableCaching)
        {
            memoryCache.Remove(ElementCacheKey(elementName));
            memoryCache.Remove(ElementNameListCacheKey);
            memoryCache.Remove(elementName + "_ReadScript");
            memoryCache.Remove(elementName + "_WriteScript");
        }
    }

    private void ClearCache(FormElement element)
    {
        if (_enableCaching)
        {
            memoryCache.Remove(ElementNameListCacheKey);
            memoryCache.Remove(ElementCacheKey(element.Name));
            if (!element.UseReadProcedure)
                memoryCache.Remove(element.Name + "_ReadScript");
            if (!element.UseWriteProcedure)
                memoryCache.Remove(element.Name + "_WriteScript");
        }
    }
}
