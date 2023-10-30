#nullable enable
using System.IO;
using System.Linq;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Python.Configuration.Options;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;

namespace JJMasterData.Python.Events;

public class PythonEventHandlerResolver : IFormEventHandlerResolver, IGridEventHandlerResolver
{
    private ScriptEngine ScriptEngine { get; }
    private string? ScriptsPath { get; }
    
    public PythonEventHandlerResolver(ScriptEngine scriptEngine,IOptions<PythonEngineOptions> options)
    {
        ScriptEngine = scriptEngine;
        ScriptsPath = options.Value.ElementScriptsPath;
    }
    
    private T? Get<T>(string elementName)
    {
        if (ScriptsPath is null)
            return default;
        
        var file = Directory
            .GetFiles(ScriptsPath, $"{elementName}.py", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (file is null)
            return default;
        
        var source = ScriptEngine.CreateScriptSourceFromFile(file);

        var compiled = source.Compile();
        compiled.Execute();

        var elementHandlerType = compiled.DefaultScope.GetVariable(elementName);

        var eventHandlerInstance = compiled.Engine.Operations.CreateInstance(elementHandlerType);

        return eventHandlerInstance is not T handler ? default : handler;
    }

    public IFormEventHandler? GetFormEventHandler(string elementName)
    {
        return Get<IFormEventHandler>(elementName);
    }

    public IGridEventHandler? GetGridEventHandler(string elementName)
    {
        return Get<IGridEventHandler>(elementName);
    }
}