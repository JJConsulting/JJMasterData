using System.Collections;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.AuditLog;

public interface IAuditLogService
{
    void AddLog(Element element,DataContext dataContext, IDictionary formValues, CommandOperation action);
    void CreateTableIfNotExist();
    string GetKey(Element element, IDictionary values);
    Element GetElement();
    FormElement GetFormElement();
}