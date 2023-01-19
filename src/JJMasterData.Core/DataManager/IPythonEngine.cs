using System.Collections.Generic;

namespace JJMasterData.Core.DataManager;

public interface IPythonEngine
{
    dynamic Execute(string script, Dictionary<string, object> args = null);
}