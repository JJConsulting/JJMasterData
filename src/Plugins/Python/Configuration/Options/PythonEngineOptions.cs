using System.Collections.Generic;

namespace JJMasterData.Python.Configuration.Options;

public class PythonEngineOptions
{
    public List<string> AdditionalScriptsPaths { get; } = new List<string>();
    public string ElementScriptsPath { get; set; }
}