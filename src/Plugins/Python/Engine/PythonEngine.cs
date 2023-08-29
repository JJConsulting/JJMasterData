using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using Microsoft.Scripting.Hosting;

namespace JJMasterData.Python.Engine;

/// <summary>
/// Class responsible for handling Python 3.4 scripts
/// </summary>
/// <remarks>
/// Gustavo Barros 05/01/2022
/// </remarks>
public class PythonEngine 
{
    private ScriptScope _scope;

    private ScriptEngine _engine;

    public ScriptEngine GetScriptEngine(Dictionary<string, object> args = null)
    {
        if (_engine == null)
        {
            Dictionary<string, object> options = new();
#if DEBUG
            options["Debug"] = true;
#endif
            _engine = IronPython.Hosting.Python.CreateEngine(options);

            _scope = _engine.CreateScope();

            var searchPaths = _engine.GetSearchPaths();

            var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)?.Substring(6);
            searchPaths.Add(Path.Combine(binPath!,"Lib"));

            _engine.SetSearchPaths(searchPaths);

            _engine.Runtime.LoadAssembly(Assembly.Load("JJMasterData.Core"));
        }

        if (args == null) return _engine;
        
        foreach (var arg in args)
        {
            _scope.SetVariable(arg.Key, arg.Value);
        }

        return _engine;
    }

    public dynamic Execute(string script, Dictionary<string, object> args = null)
    {
        return GetScriptEngine(args).CreateScriptSourceFromString(script).Execute();
    }
}
