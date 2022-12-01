using System;
using System.Collections;
using System.Data;
using JJMasterData.Commons.Dao.Providers;
using JJMasterData.Commons.Language;

namespace JJMasterData.Commons.Dao.Entity;

public class Factory : IEntityRepository
{
    private DataAccess _dataAccess;
    private BaseProvider _provider;

    internal DataAccess DataAccess
    {
        get
        {
            if (_dataAccess == null)
                _dataAccess = new DataAccess();

            return _dataAccess;
        }
    }

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

    
    public void Insert(Element element, Hashtable values) => Provider.Insert(element, values);

    
    public int Update(Element element, Hashtable values) => Provider.Update(element, values);

    
    public CommandOperation SetValues(Element element, Hashtable values) =>
        Provider.SetValues(element, values);

    
    public CommandOperation SetValues(Element element, Hashtable values, bool ignoreResults) =>
        Provider.SetValues(element, values, ignoreResults);

    
    public int Delete(Element element, Hashtable filters) => Provider.Delete(element, filters);

    
    public Hashtable GetFields(Element element, Hashtable filters) => Provider.GetFields(element, filters);

    
    public DataTable GetDataTable(Element element, IDictionary filters, string orderBy, int recordsPerPage, int currentPage, ref int totalRecords) =>
        Provider.GetDataTable(element, (Hashtable)filters, orderBy, recordsPerPage, currentPage, ref totalRecords);

    
    public DataTable GetDataTable(Element element, Hashtable filters) => Provider.GetDataTable(element,filters);

    
    public DataTable GetDataTable(string sql) => DataAccess.GetDataTable(sql);

    
    public object GetResult(string sql) => DataAccess.GetResult(sql);

    
    public void SetCommand(string sql) => DataAccess.SetCommand(sql);

    
    public int SetCommand(ArrayList sqlList) => DataAccess.SetCommand(sqlList);

    
    public bool TableExists(string tableName) => DataAccess.TableExists(tableName);

    
    public bool ExecuteBatch(string script) => DataAccess.ExecuteBatch(script);

    public int GetCount(Element element, Hashtable filters) => Provider.GetCount(element, filters);

    
    public void CreateDataModel(Element element) => Provider.CreateDataModel(element);


    public string GetScriptCreateTable(Element element) => Provider.GetScriptCreateTable(element);


    public string GetScriptWriteProcedure(Element element) => Provider.GetScriptWriteProcedure(element);


    public string GetScriptReadProcedure(Element element) => Provider.GetScriptReadProcedure(element);


    public Element GetElementFromTable(string tableName) => Provider.GetElementFromTable(tableName);


    public string GetListFieldsAsText(Element element, Hashtable filters, string orderBy, int recordsPerPage, int currentPage, bool showLogInfo, string delimiter = "|") =>
        Provider.GetListFieldsAsText(element, filters, orderBy, recordsPerPage, currentPage, showLogInfo, delimiter);

}