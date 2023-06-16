using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services.Abstractions;



public interface IDataDictionaryService
{
    public Task CreateStructureAsync()
    {
        return Task.CompletedTask;
    }
    public Task<bool> ExistsAsync(string name);
    public Task<FormElement?> GetAsync(string name);
    public Task<EntityResult<FormElement>> GetEntityResultAsync(EntityParameters? query = null);
    public Task InsertOrReplaceAsync(FormElement metadata);
    public Task DeleteAsync(string name);
}