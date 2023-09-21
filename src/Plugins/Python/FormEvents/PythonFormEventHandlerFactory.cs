using System.IO;
using System.Linq;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Python.Engine;
using JJMasterData.Python.Models;
using Microsoft.Extensions.Options;

namespace JJMasterData.Python.FormEvents;

public class PythonFormEventHandlerFactory : IFormEventHandlerFactory
{
    public string ScriptsPath { get; }
    
    public PythonFormEventHandlerFactory(IOptions<PythonFormEventOptions> options)
    {
        ScriptsPath = options.Value.ScriptsPath;
    }
    
    public IFormEventHandler GetFormEvent(string elementName)
    {
        var engine = new PythonEngine().GetScriptEngine();

        var file = Directory
            .GetFiles(ScriptsPath, $"{elementName}.py", SearchOption.AllDirectories)
            .FirstOrDefault();

        var source = engine.CreateScriptSourceFromFile(file);

        var compiled = source.Compile();
        compiled.Execute();

        var formEventPythonType = compiled.DefaultScope.GetVariable(elementName);

        var formEventInstance = compiled.Engine.Operations.CreateInstance(formEventPythonType);

        return (IFormEventHandler)formEventInstance;
    }
}