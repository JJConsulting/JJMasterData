#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Data.Entity.Services;

public interface IEntityService
{
    Task<IDictionary<string,dynamic>> GetDictionaryAsync(Element metadata, IDictionary<string, dynamic?> filters);

    Task<EntityResult<IDictionary<string, dynamic?>>> GetEntityResultAsync(Element metadata,
        EntityParameters? parameters = null);

    Task InsertAsync(Element metadata, IDictionary<string, dynamic?> values);
    Task UpdateAsync(Element metadata, IDictionary<string, dynamic?> values);
    Task DeleteAsync(Element metadata, IDictionary<string, dynamic?> filters);
    Task<bool> ExistsAsync(string name);
}