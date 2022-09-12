using System.Collections.Generic;

namespace JJMasterData.Core.FormEvents.Abstractions;

public interface IPythonEngine : IFormEventEngine
{
    dynamic Execute(string script, Dictionary<string, object> args = null);
    new IFormEvent GetFormEvent(string name);
}