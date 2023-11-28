#nullable enable

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Providers;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Data.Entity.Repository;

public class EntityRepository : IEntityRepository
{
    private ILoggerFactory LoggerFactory { get; }
    private DataAccess DataAccess { get; }
    private EntityProviderBase Provider { get; }

    public EntityRepository(DataAccess dataAccess, ILoggerFactory loggerFactory, EntityProviderBase provider)
    {
        LoggerFactory = loggerFactory;
        DataAccess = dataAccess;
        Provider = provider;
    }

    public int Update(Element element, IDictionary<string, object?> values)
    {
        return Provider.Update(element, values);
    }

    public async Task<int> DeleteAsync(Element element, IDictionary<string,object> filters) => await Provider.DeleteAsync(element, filters);
    public int Delete(Element element, IDictionary<string, object> primaryKeys)
    {
        return Provider.Delete(element, primaryKeys);
    }

    public void Insert(Element element, IDictionary<string, object?> values)
    {
        Provider.Insert(element, values);
    }

    public async Task InsertAsync(Element element, IDictionary<string,object?> values) => await Provider.InsertAsync(element, values);

    public async Task<int> UpdateAsync(Element element, IDictionary<string,object?> values) => await Provider.UpdateAsync(element, values);

    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary<string,object?> values, bool ignoreResults = false) =>
        await Provider.SetValuesAsync(element, values, ignoreResults);

    public CommandOperation SetValues(Element element, IDictionary<string, object?> values, bool ignoreResults = false)
    {
        return Provider.SetValues(element, values, ignoreResults);
    }

    public async Task<Element> GetElementFromTableAsync(string tableName) =>await Provider.GetElementFromTableAsync(tableName);
    public async Task<object?> GetResultAsync(DataAccessCommand command)
    {
        return await DataAccess.GetResultAsync(command);
    }

    public async Task<bool> TableExistsAsync(string tableName) => await DataAccess.TableExistsAsync(tableName);
    
    public async Task SetCommandAsync(DataAccessCommand command)
    {
        await DataAccess.SetCommandAsync(command);
    }
    
    public async Task<int> SetCommandListAsync(IEnumerable<DataAccessCommand> commandList) => await DataAccess.SetCommandListAsync(commandList);
    
    public async Task<bool> ColumnExistsAsync(string tableName, string columnName) => await DataAccess.ColumnExistsAsync(tableName,columnName);


    public async Task<bool> ExecuteBatchAsync(string script) => await DataAccess.ExecuteBatchAsync(script);

    public IDictionary<string, object?> GetFields(Element element, Dictionary<string, object> primaryKeys)
    {
        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetReadCommand(element,new EntityParameters
        {
            Filters = primaryKeys!
        },totalOfRecords);

        return DataAccess.GetDictionary(cmd) ?? new Dictionary<string,object?>();
    }

    public IDictionary<string, object?> GetFields(DataAccessCommand command)
    {
        return DataAccess.GetDictionary(command) ?? new Dictionary<string, object?>();
    }
    
    public async Task<IDictionary<string, object?>> GetFieldsAsync(DataAccessCommand command)
    {
        return await DataAccess.GetDictionaryAsync(command);
    }

    public async Task<IDictionary<string, object?>> GetFieldsAsync(Element element, IDictionary<string, object> primaryKeys)
    {
        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetReadCommand(element,new EntityParameters
        {
            Filters = primaryKeys!
        },totalOfRecords);

        return await DataAccess.GetDictionaryAsync(cmd);
    }

    public async Task CreateDataModelAsync(Element element) => await Provider.CreateDataModelAsync(element);

    ///<inheritdoc cref="IEntityRepository.GetCreateTableScript"/>
    public string GetCreateTableScript(Element element) => Provider.GetCreateTableScript(element);

    ///<inheritdoc cref="IEntityRepository.GetWriteProcedureScript"/>
    public string? GetWriteProcedureScript(Element element) => Provider.GetWriteProcedureScript(element);

    public async Task<string> GetAlterTableScriptAsync(Element element)
    {
        var addedFields = await GetAddedFieldsAsync(element).ToListAsync();
        return Provider.GetAlterTableScript(element, addedFields);
    }
    
    private async IAsyncEnumerable<ElementField> GetAddedFieldsAsync(Element element)
    {
        if (!await TableExistsAsync(element.TableName))
            yield break;
        
        foreach (var field in element.Fields.Where(f => f.DataBehavior == FieldBehavior.Real))
        {
            if (!await ColumnExistsAsync(element.TableName, field.Name))
            {
                yield return field;
            }
        }
    }
    
    public string? GetReadProcedureScript(Element element) => Provider.GetReadProcedureScript(element);
    

    public async Task<string> GetListFieldsAsTextAsync(Element element, EntityParameters? parameters = null, bool showLogInfo = false,
        string delimiter = "|")
    {
        return await Provider.GetFieldsListAsTextAsync(element, parameters ?? new EntityParameters(), showLogInfo, delimiter);
    }
    
    public async Task<List<Dictionary<string,object?>>> GetDictionaryListAsync(
        Element element,
        EntityParameters? parameters = null
    )
    {
        var result = await Provider.GetDictionaryListAsync(element, parameters ?? new EntityParameters(), false);
        
        return result.Data;
    }

    public async Task<DataTable> GetDataTableAsync(Element element, EntityParameters? parameters = null)
    {
        var totalOfRecords =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var command = Provider.GetReadCommand(element, parameters ?? new EntityParameters(), totalOfRecords);

        return await DataAccess.GetDataTableAsync(command);
    }

    public int GetCount(Element element, IDictionary<string,object?> values)
    {
        var result = GetDictionaryListResult(element, new EntityParameters
        {
            Filters = values
        });

        return result.Count;
    }

    public async Task<int> GetCountAsync(Element element, IDictionary<string, object?> filters)
    {
        var result = await GetDictionaryListResultAsync(element, new EntityParameters
        {
            Filters = filters
        });

        return result.Count;
    }

    public async Task<List<Dictionary<string,object?>>> GetDictionaryListAsync(DataAccessCommand command)
    {
        return await DataAccess.GetDictionaryListAsync(command);
    }
    
    public async Task<DictionaryListResult> GetDictionaryListResultAsync(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
    )
    {
        var result = await Provider.GetDictionaryListAsync(element, parameters ?? new EntityParameters(), recoverTotalOfRecords);
        
        return new DictionaryListResult(result.Data,result.TotalOfRecords);
    }
    
    public DictionaryListResult GetDictionaryListResult(
        Element element,
        EntityParameters? parameters = null,
        bool recoverTotalOfRecords = true
    )
    {
        var result = Provider.GetDictionaryList(element, parameters ?? new EntityParameters(), recoverTotalOfRecords);
        
        return new DictionaryListResult(result.Data,result.TotalOfRecords);
    }
    

   
}