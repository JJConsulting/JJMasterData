using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class DataDictionaryService : IDataDictionaryService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }

    public DataDictionaryService(IDataDictionaryRepository dataDictionaryRepository)
    {
        DataDictionaryRepository = dataDictionaryRepository;
    }
        
    public FormElement GetMetadata(string dictionaryName)
    {
        return DataDictionaryRepository.GetMetadata(dictionaryName);
    }

    public async Task<FormElement> GetMetadataAsync(string dictionaryName)
    {
        
        
        return await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
    }

    public bool Exists(string dictionaryName)
    {
        return DataDictionaryRepository.Exists(dictionaryName);
    }

    public async Task<bool> ExistsAsync(string dictionaryName)
    {
        return await DataDictionaryRepository.ExistsAsync(dictionaryName);
    }

    public void InsertOrReplace(FormElement formElement)
    {
        DataDictionaryRepository.InsertOrReplace(formElement);
    }

    public async Task InsertOrReplaceAsync(FormElement metadata)
    {
        await DataDictionaryRepository.InsertOrReplaceAsync(metadata);
    }

    public void Delete(string dictionaryName)
    {
        DataDictionaryRepository.Delete(dictionaryName);
    }

    public async Task DeleteAsync(string dictionaryName)
    {
        await DataDictionaryRepository.DeleteAsync(dictionaryName);
    }
}