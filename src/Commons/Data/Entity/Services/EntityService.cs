#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using System.Collections;

namespace JJMasterData.Commons.Data.Entity.Services;

public class EntityService : IEntityService
{
    private IEntityRepository EntityRepository { get; }

    public EntityService(IEntityRepository entityRepository)
    {
        EntityRepository = entityRepository;
    }
    
    public async Task<IDictionary<string,dynamic>> GetDictionaryAsync(Element metadata, IDictionary<string, dynamic?> filters)
    {
        return await EntityRepository.GetDictionaryAsync(metadata, filters );
    }

    public async Task<EntityResult<IDictionary<string, dynamic?>>> GetEntityResultAsync(Element metadata,
        EntityParameters? parameters = null)
    {
        var result = await EntityRepository.GetDictionaryListAsync(metadata, parameters?.Parameters as IDictionary,
            parameters?.OrderBy?.ToString(), parameters?.PaginationData?.RecordsPerPage ?? 5,
            parameters?.PaginationData?.Page ?? 1, 0);
        
        return new EntityResult<IDictionary<string, dynamic?>>(result.Item1, result.Item2);
    }

    public async Task InsertAsync(Element metadata, IDictionary<string, dynamic?> values)
    {
        await EntityRepository.InsertAsync(metadata, values as IDictionary);
    }

    public async Task UpdateAsync(Element metadata, IDictionary<string, dynamic?> values)
    {
        await EntityRepository.UpdateAsync(metadata, values as IDictionary);
    }

    public async Task DeleteAsync(Element metadata, IDictionary<string, dynamic?> filters)
    {
        await EntityRepository.DeleteAsync(metadata, filters as IDictionary);
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await EntityRepository.TableExistsAsync(name);
    }

 
}