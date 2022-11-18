using System.Collections;
using System.Data;

namespace JJMasterData.Commons.Dao.Entity;

public interface IProvider
{
    string VariablePrefix { get; }
    string GetCreateTableScript(Element element);
    string GetWriteProcedureScript(Element element);
    string GetReadProcedureScript(Element element);

    DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref int tot, ref DataAccess dataAccess);
    DataAccessCommand GetInsertScript(Element element, Hashtable values);
    DataAccessCommand GetUpdateScript(Element element, Hashtable values);
    DataAccessCommand GetDeleteScript(Element element, Hashtable filters);
    DataAccessCommand GetReadCommand(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref DataAccessParameter pTot);
    DataAccessCommand GetWriteCommand(string action, Element element, Hashtable values);
       
}