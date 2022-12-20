using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary;

public interface IPythonEngine
{
    dynamic Execute(string script, Dictionary<string, object> args = null);
}