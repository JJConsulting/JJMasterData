using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFormFileService
{
    void SaveFormMemoryFiles(FormElement formElement, IDictionary<string, object> primaryKeys);
    void DeleteFiles(FormElement formElement, IDictionary<string, object> primaryKeys);
}