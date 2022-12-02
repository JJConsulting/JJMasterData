using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JJMasterData.Core.DataDictionary;
using Microsoft.Scripting.Hosting;

namespace JJMasterData.Python;

/// <summary>
/// Class responsible for handling Python 3.4 scripts
/// </summary>
/// <remarks>
/// Gustavo Barros 05/01/2022
/// </remarks>
public class PythonEngine : IPythonEngine
{
    public ScriptScope Scope;

    private ScriptEngine _engine;

    private ScriptEngine GetEngine(Dictionary<string, object> args = null)
    {
        if (_engine == null)
        {
            Dictionary<string, object> options = new();
#if DEBUG
            options["Debug"] = true;
#endif
            _engine = IronPython.Hosting.Python.CreateEngine(options);

            Scope = _engine.CreateScope();

            var searchPaths = _engine.GetSearchPaths();

            string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)?.Substring(6);
            searchPaths.Add(Path.Combine(binPath,"Lib"));

            _engine.SetSearchPaths(searchPaths);

            _engine.Runtime.LoadAssembly(Assembly.Load("JJMasterData.Core"));
        }

        if (args == null) return _engine;
        
        foreach (var arg in args)
        {
            Scope.SetVariable(arg.Key, arg.Value);
        }

        return _engine;
    }

    public dynamic Execute(string script, Dictionary<string, object> args = null)
    {
        return GetEngine(args).CreateScriptSourceFromString(script).Execute();
    }
}
