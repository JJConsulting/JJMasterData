using System.IO;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Python;
public static class Extensions
{
    public static JJServiceBuilder WithPythonEngine(this JJServiceBuilder builder)
    {
        builder.WithPythonEngine<PythonEngine>();
        return builder;
    }
}