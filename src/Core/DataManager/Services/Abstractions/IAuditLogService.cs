using System.Collections;
using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IAuditLogService
{
    void AddLog(Element element,DataContext dataContext, IDictionary<string,dynamic>formValues, CommandOperation action);
    void CreateTableIfNotExist();
    string GetKey(Element element, IDictionary<string,dynamic>values);
    Element GetElement();
    FormElement GetFormElement();
}