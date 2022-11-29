using System.Collections;
using System.Data;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;

namespace JJMasterData.MongoDB.Repository;

/// <summary>
/// TODO: Refactor IEntityRepository to be able to use with any data source.
/// </summary>
internal class MongoEntityRepository : IEntityRepository
{
    public string GetListFieldsAsText(Element element, Hashtable filters, string orderby, int regporpag, int pag, bool showLogInfo,
        string delimiter = "|")
    {
        throw new NotImplementedException();
    }

    public DataTable GetDataTable(Element element, IDictionary filters, string orderby, int regperpage, int pag, ref int tot)
    {
        throw new NotImplementedException();
    }

    public DataTable GetDataTable(Element element, Hashtable filters)
    {
        throw new NotImplementedException();
    }

    public Hashtable GetFields(Element element, Hashtable filters)
    {
        throw new NotImplementedException();
    }

    public int GetCount(Element element, Hashtable filters)
    {
        throw new NotImplementedException();
    }

    public int Update(Element element, Hashtable values)
    {
        throw new NotImplementedException();
    }

    public int Delete(Element element, Hashtable filters)
    {
        throw new NotImplementedException();
    }

    public void Insert(Element element, Hashtable values)
    {
        throw new NotImplementedException();
    }

    public CommandOperation SetValues(Element element, Hashtable values)
    {
        throw new NotImplementedException();
    }

    public CommandOperation SetValues(Element element, Hashtable values, bool ignoreResults)
    {
        throw new NotImplementedException();
    }

    public void CreateDataModel(Element element)
    {
        throw new NotImplementedException();
    }

    public string GetScriptCreateTable(Element element)
    {
        throw new NotImplementedException();
    }

    public string GetScriptReadProcedure(Element element)
    {
        throw new NotImplementedException();
    }

    public string GetScriptWriteProcedure(Element element)
    {
        throw new NotImplementedException();
    }

    public Element GetElementFromTable(string tableName)
    {
        throw new NotImplementedException();
    }

    public DataTable GetDataTable(string sql)
    {
        throw new NotImplementedException();
    }

    public object GetResult(string sql)
    {
        throw new NotImplementedException();
    }

    public bool TableExists(string tableName)
    {
        throw new NotImplementedException();
    }

    public void SetCommand(string sql)
    {
        throw new NotImplementedException();
    }

    public int SetCommand(ArrayList sqlList)
    {
        throw new NotImplementedException();
    }

    public bool ExecuteBatch(string script)
    {
        throw new NotImplementedException();
    }
}