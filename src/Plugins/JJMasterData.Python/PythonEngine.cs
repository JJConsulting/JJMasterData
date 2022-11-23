using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Logging;
using JJMasterData.Core.FormEvents.Abstractions;
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
    public string ScriptsPath { get; private set; }

    private static PythonEngine _instance;
    private PythonEngine(string scriptsPath)
    {
        ScriptsPath = scriptsPath;
    }

    public static PythonEngine GetInstance(string scriptsPath) => _instance ??= new PythonEngine(scriptsPath);

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

            searchPaths.Add(ScriptsPath);

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

    public IFormEvent GetFormEvent(string name)
    {
        try
        {
            var engine = GetEngine();

            var file = Directory.GetFiles(ScriptsPath, name + ".py", SearchOption.AllDirectories).FirstOrDefault();

            var source = engine.CreateScriptSourceFromFile(file);

            var compiled = source.Compile();
            compiled.Execute();

            compiled.DefaultScope.SetVariable("DataAccess", JJService.EntityRepository);

            var formEventPythonType = compiled.DefaultScope.GetVariable(name);

            var formEventInstance = compiled.Engine.Operations.CreateInstance(formEventPythonType);

            return (IFormEvent)formEventInstance;
        }
        catch (Exception e)
        {
            Log.AddError(e.ToString());
            return null;
        }
        
    }

    public dynamic Execute(string script, Dictionary<string, object> args = null)
    {
        return GetEngine(args).CreateScriptSourceFromString(script).Execute();
    }
}
