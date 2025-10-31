using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Scripting.Hosting;

namespace JJMasterData.Python.Engine;

/// <summary>
/// Class responsible for handling Python 3.4 scripts
/// </summary>
/// <remarks>
/// Gustavo Barros 05/01/2022
/// </remarks>
public static class PythonEngineFactory
{
    public static ScriptEngine CreateScriptEngine(IEnumerable<string> additionalPaths)
    {
        Dictionary<string, object> options = new();
#if DEBUG
        options["Debug"] = true;
#endif
        var engine = IronPython.Hosting.Python.CreateEngine(options);

        var searchPaths = engine.GetSearchPaths();

        
        var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        searchPaths.Add(Path.Combine(binPath!, "Lib"));

        foreach (var additionalPath in additionalPaths)
            searchPaths.Add(additionalPath);
        
        engine.SetSearchPaths(searchPaths);

        engine.Runtime.LoadAssembly(Assembly.Load("JJMasterData.Core"));

        return engine;
    }
}