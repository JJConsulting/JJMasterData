#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Providers;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity;

public class EntityRepository : IEntityRepository
{
    private JJMasterDataCommonsOptions Options { get; }
    private DataAccess DataAccess { get; }
    private BaseProvider Provider { get; }
    
    [ActivatorUtilitiesConstructor]
    public EntityRepository(IConfiguration configuration, IOptions<JJMasterDataCommonsOptions> options)
    {
        var connectionString = configuration.GetConnectionString("ConnectionString");
        var connectionProvider = configuration.GetSection("ConnectionProviders").GetValue<string?>("ConnectionString") ?? "SqlServer";
        DataAccess = new DataAccess(connectionString, connectionProvider);
        Options = options.Value;
        Provider = GetProvider();
    }
    
    public EntityRepository(string connectionString, DataAccessProvider provider, IOptions<JJMasterDataCommonsOptions> options)
    {
        DataAccess = new DataAccess(connectionString, provider);
        Options = options.Value;

        Provider = GetProvider();
    }

    private BaseProvider GetProvider()
    {
        return DataAccess.ConnectionProvider switch
        {
            DataAccessProvider.SqlServer => new SqlServerProvider(DataAccess, Options),
            DataAccessProvider.Oracle => new OracleProvider(DataAccess, Options),
            DataAccessProvider.OracleNetCore => new OracleProvider(DataAccess, Options),
            DataAccessProvider.SqLite => new ProviderSQLite(DataAccess, Options),
            _ => throw new InvalidOperationException(Translate.Key("Invalid data provider.") + " [" + DataAccess.ConnectionProvider + "]")
        };
    }

    public async Task<int> DeleteAsync(Element element, IDictionary filters) => await Provider.DeleteAsync(element, filters);

    ///<inheritdoc cref="IEntityRepository.Insert(Element, IDictionary)"/>
    public void Insert(Element element, IDictionary values) => Provider.Insert(element, values);

    public async Task InsertAsync(Element element, IDictionary values) => await Provider.InsertAsync(element, values);

    public async Task<int> GetCountAsync(Element element, IDictionary filters) => await Provider.GetCountAsync(element, filters);

    ///<inheritdoc cref="IEntityRepository.Update(Element, IDictionary)"/>
    public int Update(Element element, IDictionary values) => Provider.Update(element, values);

    public async Task<int> UpdateAsync(Element element, IDictionary values) => await Provider.UpdateAsync(element, values);

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary)"/>
    public CommandOperation SetValues(Element element, IDictionary values) =>
        Provider.SetValues(element,values);

    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary values) =>
       await Provider.SetValuesAsync(element, values);

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary, bool)"/>
    public CommandOperation SetValues(Element element, IDictionary values, bool ignoreResults) =>
        Provider.SetValues(element, values, ignoreResults);

    public async Task<CommandOperation> SetValuesAsync(Element element, IDictionary values, bool ignoreResults) =>
        await Provider.SetValuesAsync(element, values, ignoreResults);

    ///<inheritdoc cref="IEntityRepository.Delete(Element, IDictionary)"/>
    public int Delete(Element element, IDictionary filters) => Provider.Delete(element, filters);

    public async Task<DataTable> GetDataTableAsync(Element element, IDictionary filters) =>
        await Provider.GetDataTableAsync(element, filters);

    ///<inheritdoc cref="IEntityRepository.GetFields(Element, IDictionary)"/>
    public Hashtable GetFields(Element element, IDictionary filters) => Provider.GetFields(element,filters);

    public async Task<Hashtable> GetFieldsAsync(Element element, IDictionary filters)=>await Provider.GetFieldsAsync(element,filters);
    

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element,System.Collections.IDictionary,string,int,int,ref int)"/>
    public DataTable GetDataTable(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords) =>
        Provider.GetDataTable(element, filters, orderBy, recordsPerPage, currentPage, ref totalRecords);

    public async Task<(DataTable, int)> GetDataTableAsync(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage,
        int totalRecords)
    {
        return await Provider.GetDataTableAsync(element, filters, orderBy, recordsPerPage, currentPage, totalRecords);
    }
    
    public async Task<(List<Dictionary<string, dynamic>>, int)> GetDictionaryListAsync(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage,
        int totalRecords)
    {
        return await Provider.GetDictionaryListAsync(element, filters, orderBy, recordsPerPage, currentPage, totalRecords);
    }

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, IDictionary)"/>
    public DataTable GetDataTable(Element element, IDictionary filters) => Provider.GetDataTable(element,filters);

    public async Task<Element> GetElementFromTableAsync(string tableName) =>await Provider.GetElementFromTableAsync(tableName);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(string)"/>
    public DataTable GetDataTable(string sql) => DataAccess.GetDataTable(sql);

    public async Task<DataTable> GetDataTableAsync(string sql) => await DataAccess.GetDataTableAsync(sql);

    ///<inheritdoc cref="IEntityRepository.GetResult(string)"/>
    public object GetResult(string sql) => DataAccess.GetResult(sql);

    public async Task<object> GetResultAsync(string sql) => await DataAccess.GetResultAsync(sql);

    public async Task<bool> TableExistsAsync(string tableName) => await DataAccess.TableExistsAsync(tableName);

    public void SetCommand(string sql) => DataAccess.SetCommand(sql);

    public async Task SetCommandAsync(string sql) => await DataAccess.SetCommandAsync(sql);

    ///<inheritdoc cref="IEntityRepository.SetCommand(IEnumerable)"/>
    public int SetCommand(IEnumerable<string> sqlList) => DataAccess.SetCommand(sqlList);

    public async Task<int> SetCommandAsync(IEnumerable<string> sqlList) => await DataAccess.SetCommandAsync(sqlList);

    ///<inheritdoc cref="IEntityRepository.TableExists(string)"/>
    public bool TableExists(string tableName) => DataAccess.TableExists(tableName);

    ///<inheritdoc cref="IEntityRepository.ExecuteBatch(string)"/>
    public bool ExecuteBatch(string script) => DataAccess.ExecuteBatch(script);

    public async Task<bool> ExecuteBatchAsync(string script) => await DataAccess.ExecuteBatchAsync(script);
    public async Task<IDictionary<string, dynamic>> GetDictionaryAsync(Element metadata, IDictionary<string, dynamic> filters)
    {
        var total =
            new DataAccessParameter("@qtdtotal", 1, DbType.Int32, 0, ParameterDirection.InputOutput);
        var cmd = Provider.GetReadCommand(metadata, filters as IDictionary, "", 1, 1, ref total);

        return await DataAccess.GetDictionaryAsync(cmd);
    }

    ///<inheritdoc cref="IEntityRepository.GetCount(Element, IDictionary)"/>
    public int GetCount(Element element, IDictionary filters) => Provider.GetCount(element, filters);

    ///<inheritdoc cref="IEntityRepository.CreateDataModel(Element)"/>
    public void CreateDataModel(Element element) => Provider.CreateDataModel(element);

    public async Task CreateDataModelAsync(Element element) => await Provider.CreateDataModelAsync(element);

    ///<inheritdoc cref="IEntityRepository.GetScriptCreateTable(Element)"/>
    public string GetScriptCreateTable(Element element) => Provider.GetCreateTableScript(element);

    ///<inheritdoc cref="IEntityRepository.GetScriptWriteProcedure(Element)"/>
    public string GetScriptWriteProcedure(Element element) => Provider.GetWriteProcedureScript(element);

    ///<inheritdoc cref="IEntityRepository.GetScriptReadProcedure(Element)"/>
    public string GetScriptReadProcedure(Element element) => Provider.GetReadProcedureScript(element);

    ///<inheritdoc cref="IEntityRepository.GetElementFromTable(string)"/>
    public Element GetElementFromTable(string tableName) => Provider.GetElementFromTable(tableName);

    ///<inheritdoc cref="IEntityRepository.GetListFieldsAsText(Element,IDictionary,string,int,int,bool,string)"/>
    public string GetListFieldsAsText(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, bool showLogInfo, string delimiter = "|") =>
        Provider.GetListFieldsAsText(element, filters, orderBy, recordsPerPage, currentPage, showLogInfo, delimiter);

}