using System.Collections.Generic;

namespace JJMasterData.Python.Configuration.Options;

public class PythonEngineOptions
{
    public List<string> AdditionalScriptsPaths { get; } = [];
    public string ElementScriptsPath { get; set; }
}