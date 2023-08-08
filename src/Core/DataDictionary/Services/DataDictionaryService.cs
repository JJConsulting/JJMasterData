using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class DataDictionaryService : IDataDictionaryService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEnumerable<IFormElementFactory> Factories { get; }

    public DataDictionaryService(
        IDataDictionaryRepository dataDictionaryRepository,
        IEnumerable<IFormElementFactory> factories)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        Factories = factories;
    }

    private IFormElementFactory GetFactory(string elementName)
    {
        return Factories.First(f => f.ElementName == elementName);
    }
    
    public FormElement GetMetadata(string elementName)
    {
        var formElement = DataDictionaryRepository.GetMetadata(elementName);

        if (formElement is null)
        {
            var factory = GetFactory(elementName);
            return factory.GetFormElement();
        }

        return formElement;
    }

    public async Task<FormElement> GetMetadataAsync(string elementName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementName);

        if (formElement is null)
        {
            var factory = GetFactory(elementName);
            return factory.GetFormElement();
        }

        return formElement;
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