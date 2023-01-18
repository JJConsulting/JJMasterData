using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Data.Providers;
using JJMasterData.Commons.Localization;

namespace JJMasterData.Commons.Data.Entity;

public class EntityRepository : IEntityRepository
{
    private DataAccess _dataAccess;
    private BaseProvider _provider;

    internal DataAccess DataAccess => _dataAccess ??= new DataAccess();

    internal BaseProvider Provider
    {
        get
        {
            if (_provider != null) 
                return _provider;

            _provider = DataAccessProvider.GetDataAccessProviderTypeFromString(DataAccess.ConnectionProvider) switch
            {
                DataAccessProviderType.SqlServer => new MSSQLProvider(DataAccess),
                DataAccessProviderType.Oracle => new OracleProvider(DataAccess),
                DataAccessProviderType.OracleNetCore => new OracleProvider(DataAccess),
                DataAccessProviderType.SqLite => new ProviderSQLite(DataAccess),
                _ => throw new InvalidOperationException(Translate.Key("Invalid data provider.") + " [" +
                                                         DataAccess.ConnectionProvider + "]")
            };

            return _provider;
        }
    }

    ///<inheritdoc cref="IEntityRepository.Insert(Element, IDictionary)"/>
    public void Insert(Element element, IDictionary values) => Provider.Insert(element, values);

    ///<inheritdoc cref="IEntityRepository.Update(Element, IDictionary)"/>
    public int Update(Element element, IDictionary values) => Provider.Update(element, values);

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary)"/>
    public CommandOperation SetValues(Element element, IDictionary values) =>
        Provider.SetValues(element,values);

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, IDictionary, bool)"/>
    public CommandOperation SetValues(Element element, IDictionary values, bool ignoreResults) =>
        Provider.SetValues(element, values, ignoreResults);

    ///<inheritdoc cref="IEntityRepository.Delete(Element, IDictionary)"/>
    public int Delete(Element element, IDictionary filters) => Provider.Delete(element, filters);

    ///<inheritdoc cref="IEntityRepository.GetFields(Element, IDictionary)"/>
    public Hashtable GetFields(Element element, IDictionary filters) => Provider.GetFields(element,filters);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element,System.Collections.IDictionary,string,int,int,ref int)"/>
    public DataTable GetDataTable(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords) =>
        Provider.GetDataTable(element, filters, orderBy, recordsPerPage, currentPage, ref totalRecords);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, IDictionary)"/>
    public DataTable GetDataTable(Element element, IDictionary filters) => Provider.GetDataTable(element,filters);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(string)"/>
    public DataTable GetDataTable(string sql) => DataAccess.GetDataTable(sql);

    ///<inheritdoc cref="IEntityRepository.GetResult(string)"/>
    public object GetResult(string sql) => DataAccess.GetResult(sql);

    ///<inheritdoc cref="IEntityRepository.SetCommand(string)"/>
    public void SetCommand(string sql) => DataAccess.SetCommand(sql);

    ///<inheritdoc cref="IEntityRepository.SetCommand(ArrayList)"/>
    public int SetCommand(IEnumerable<string> sqlList) => DataAccess.SetCommand(sqlList);

    ///<inheritdoc cref="IEntityRepository.TableExists(string)"/>
    public bool TableExists(string tableName) => DataAccess.TableExists(tableName);

    ///<inheritdoc cref="IEntityRepository.ExecuteBatch(string)"/>
    public bool ExecuteBatch(string script) => DataAccess.ExecuteBatch(script);

    ///<inheritdoc cref="IEntityRepository.GetCount(Element, Hashtable)"/>
    public int GetCount(Element element, IDictionary filters) => Provider.GetCount(element, filters);

    ///<inheritdoc cref="IEntityRepository.CreateDataModel(Element)"/>
    public void CreateDataModel(Element element) => Provider.CreateDataModel(element);

    ///<inheritdoc cref="IEntityRepository.GetScriptCreateTable(Element)"/>
    public string GetScriptCreateTable(Element element) => Provider.GetScriptCreateTable(element);

    ///<inheritdoc cref="IEntityRepository.GetScriptWriteProcedure(Element)"/>
    public string GetScriptWriteProcedure(Element element) => Provider.GetScriptWriteProcedure(element);

    ///<inheritdoc cref="IEntityRepository.GetScriptReadProcedure(Element)"/>
    public string GetScriptReadProcedure(Element element) => Provider.GetScriptReadProcedure(element);

    ///<inheritdoc cref="IEntityRepository.GetElementFromTable(string)"/>
    public Element GetElementFromTable(string tableName) => Provider.GetElementFromTable(tableName);

    ///<inheritdoc cref="IEntityRepository.GetListFieldsAsText(Element,IDictionary,string,int,int,bool,string)"/>
    public string GetListFieldsAsText(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, bool showLogInfo, string delimiter = "|") =>
        Provider.GetListFieldsAsText(element, filters, orderBy, recordsPerPage, currentPage, showLogInfo, delimiter);

}