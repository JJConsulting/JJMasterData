using System.Collections;
using System.Data;

namespace JJMasterData.Commons.Dao.Entity
{
    public interface IEntityRepository
    {
        string GetListFieldsAsText(Element element, Hashtable filters, string orderby, int regporpag, int pag, bool showLogInfo, string delimiter = "|");
        DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regperpage, int pag, ref int tot);
        DataTable GetDataTable(Element element, Hashtable filters);
        Hashtable GetFields(Element element, Hashtable filters);
        int GetCount(Element element, Hashtable filters);

        int Update(Element element, Hashtable values);
        int Delete(Element element, Hashtable filters);
        void Insert(Element element, Hashtable values);
        CommandType SetValues(Element element, Hashtable values);
        CommandType SetValues(Element element, Hashtable values, bool ignoreResults);

        void CreateDataModel(Element element);
        string GetCreateTableScript(Element element);
        string GetReadProcedureScript(Element element);
        string GetWriteProcedureScript(Element element);
        Element GetElementFromTable(string tableName);

        DataTable GetDataTable(string sql);
        object GetResult(string sql);
        bool TableExists(string tableName);
        void SetCommand(string sql);
        int SetCommand(ArrayList sqlList);
        bool ExecuteBatch(string script);

    }
}