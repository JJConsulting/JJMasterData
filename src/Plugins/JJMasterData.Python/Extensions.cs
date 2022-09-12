using System.IO;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Python;
public static class Extensions
{
    public static JJServiceBuilder WithPythonEngine(this JJServiceBuilder builder)
    {
        string executingCodeBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
        return WithPythonEngine(builder, executingCodeBase);
    }
    public static JJServiceBuilder WithPythonEngine(this JJServiceBuilder builder, string scriptsPath)
    {
        builder.WithPythonEngine(PythonEngine.GetInstance(scriptsPath));
        return builder;
    }
}