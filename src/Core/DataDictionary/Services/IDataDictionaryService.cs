using System.Threading.Tasks;

namespace JJMasterData.Core.DataDictionary.Services;

public interface IDataDictionaryService
{
    FormElement GetMetadata(string elementName);
    Task<FormElement> GetMetadataAsync(string elementName);
    bool Exists(string dictionaryName);
    Task<bool> ExistsAsync(string dictionaryName);
    void InsertOrReplace(FormElement formElement);
    Task InsertOrReplaceAsync(FormElement metadata);
    void Delete(string dictionaryName);
    Task DeleteAsync(string dictionaryName);
}