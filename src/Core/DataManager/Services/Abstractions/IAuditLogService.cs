using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services.Abstractions;

public interface IAuditLogService
{
    Task LogAsync(Element element,DataContext dataContext, IDictionary<string, object?>formValues, CommandOperation action);
    Task CreateTableIfNotExistsAsync();
    string GetKey(Element element, IDictionary<string, object?>values);
    Element GetElement();
    FormElement GetFormElement();
}