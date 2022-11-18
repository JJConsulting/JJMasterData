using System;
using System.Collections;
using System.Data;
using JJMasterData.Commons.Dao.Entity.Providers;
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
            if (_provider != null) return _provider;

            _provider = DataAccess.ConnectionProvider switch
            {
                DataAccessProvider.MSSQL => new MSSQLProvider(DataAccess),
                DataAccessProvider.Oracle => new OracleProvider(DataAccess),
                DataAccessProvider.SQLite => new ProviderSQLite(DataAccess),
                _ => throw new InvalidOperationException(Translate.Key("Invalid data provider.") + " [" +
                                                         DataAccess.ConnectionProvider + "]")
            };

            return _provider;
        }
    }

    public Factory()
    {
        
    }

    ///<inheritdoc cref="IEntityRepository.Insert(Element, Hashtable)"/>
    public void Insert(Element element, Hashtable values) => Provider.Insert(element, values);

    ///<inheritdoc cref="IEntityRepository.Update(Element, Hashtable)"/>
    public int Update(Element element, Hashtable values) => Provider.Update(element, values);

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, Hashtable)"/>
    public CommandOperation SetValues(Element element, Hashtable values) =>
        Provider.SetValues(element, values);

    ///<inheritdoc cref="IEntityRepository.SetValues(Element, Hashtable, bool)"/>
    public CommandOperation SetValues(Element element, Hashtable values, bool ignoreResults) =>
        Provider.SetValues(element, values, ignoreResults);

    ///<inheritdoc cref="IEntityRepository.Delete(Element, Hashtable)"/>
    public int Delete(Element element, Hashtable filters) => Provider.Delete(element, filters);

    ///<inheritdoc cref="IEntityRepository.GetFields(Element, Hashtable)"/>
    public Hashtable GetFields(Element element, Hashtable filters) => Provider.GetFields(element, filters);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable, string, int, int, ref int)"/>
    public DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regperpage, int pag, ref int tot) =>
        Provider.GetDataTable(element, filters, orderby, regperpage, pag, ref tot);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(Element, Hashtable)"/>
    public DataTable GetDataTable(Element element, Hashtable filters) => Provider.GetDataTable(element, filters);

    ///<inheritdoc cref="IEntityRepository.GetDataTable(string)"/>
    public DataTable GetDataTable(string sql) => DataAccess.GetDataTable(sql);

    ///<inheritdoc cref="IEntityRepository.GetResult(string)"/>
    public object GetResult(string sql) => DataAccess.GetResult(sql);

    ///<inheritdoc cref="IEntityRepository.SetCommand(string)"/>
    public void SetCommand(string sql) => DataAccess.SetCommand(sql);

    ///<inheritdoc cref="IEntityRepository.SetCommand(ArrayList)"/>
    public int SetCommand(ArrayList sqlList) => DataAccess.SetCommand(sqlList);

    ///<inheritdoc cref="IEntityRepository.TableExists(string)"/>
    public bool TableExists(string tableName) => DataAccess.TableExists(tableName);

    ///<inheritdoc cref="IEntityRepository.ExecuteBatch(string)"/>
    public bool ExecuteBatch(string script) => DataAccess.ExecuteBatch(script);

    ///<inheritdoc cref="IEntityRepository.GetCount(Element, Hashtable)"/>
    public int GetCount(Element element, Hashtable filters) => Provider.GetCount(element, filters);

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

    ///<inheritdoc cref="IEntityRepository.GetListFieldsAsText(Element, Hashtable, string, int, int, bool, string)"/>
    public string GetListFieldsAsText(Element element, Hashtable filters, string orderby, int regporpag, int pag, bool showLogInfo, string delimiter = "|") =>
        Provider.GetListFieldsAsText(element, filters, orderby, regporpag, pag, showLogInfo, delimiter);

}