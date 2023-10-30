#nullable enable
using System.IO;
using System.Linq;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;
using JJMasterData.Python.Configuration.Options;
using JJMasterData.Python.Engine;
using Microsoft.Extensions.Options;
using Microsoft.Scripting.Hosting;

namespace JJMasterData.Python.Events;

    
public class PythonEventHandlerFactory : IFormEventHandlerFactory, IGridEventHandlerFactory
{
    private ScriptEngine ScriptEngine { get; }
    private string ScriptsPath { get; }
    
    public PythonEventHandlerFactory(ScriptEngine scriptEngine,IOptions<PythonEngineOptions> options)
    {
        ScriptEngine = scriptEngine;
        ScriptsPath = options.Value.ElementScriptsPath;
    }
    
    private T? Get<T>(string elementName)
    {
        var file = Directory
            .GetFiles(ScriptsPath, $"{elementName}.py", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (file is not null)
        {
            var source = ScriptEngine.CreateScriptSourceFromFile(file);

            var compiled = source.Compile();
            compiled.Execute();

            var elementHandlerType = compiled.DefaultScope.GetVariable(elementName);

            var eventHandlerInstance = compiled.Engine.Operations.CreateInstance(elementHandlerType);

            if (eventHandlerInstance is T handler)
            {
                return handler;
            }
        }

        return default;
    }

    public IFormEventHandler? GetFormEvent(string elementName)
    {
        return Get<IFormEventHandler>(elementName);
    }

    public IGridEventHandler? GetGridEventHandler(string elementName)
    {
        return Get<IGridEventHandler>(elementName);
    }
}