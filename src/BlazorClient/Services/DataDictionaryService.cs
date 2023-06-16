using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.BlazorClient.Services;

public class DataDictionaryService : IDataDictionaryService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }

    public DataDictionaryService(IDataDictionaryRepository dataDictionaryRepository)
    {
        DataDictionaryRepository = dataDictionaryRepository;
    }
    
    public async Task<bool> ExistsAsync(string name)
    {
        return await DataDictionaryRepository.ExistsAsync(name);
    }

    public async Task<FormElement?> GetAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), Translate.Key("Dictionary name cannot be null."));
        return await DataDictionaryRepository.GetMetadataAsync(name);
    }

    public Task<EntityResult<FormElement>> GetEntityResultAsync(EntityParameters? query = null)
    {
        throw new NotImplementedException();
        //return await DataDictionaryRepository.GetMetadataListAsync(name);
    }

    public async Task InsertAsync(FormElement metadata)
    {
        await DataDictionaryRepository.InsertOrReplaceAsync(metadata);
    }

    public async Task InsertOrReplaceAsync(FormElement metadata)
    {
        await DataDictionaryRepository.InsertOrReplaceAsync(metadata);
    }

    public async Task DeleteAsync(string name)
    {
        await DataDictionaryRepository.DeleteAsync(name);
    }
}