using System.Collections;
using System.Data;

namespace JJMasterData.Commons.Dao.Entity;

public interface IProvider
{
    string VariablePrefix { get; }
    string GetScriptCreateTable(Element element);
    string GetScriptWriteProcedure(Element element);
    string GetScriptReadProcedure(Element element);

    DataTable GetDataTable(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref int tot, ref DataAccess dataAccess);
    DataAccessCommand GetCommandInsert(Element element, Hashtable values);
    DataAccessCommand GetCommandUpdate(Element element, Hashtable values);
    DataAccessCommand GetCommandDelete(Element element, Hashtable filters);
    DataAccessCommand GetCommandRead(Element element, Hashtable filters, string orderby, int regporpag, int pag, ref DataAccessParameter pTot);
    DataAccessCommand GetCommandInsertOrReplace(Element element, Hashtable values);
       
}