#nullable enable

namespace JJMasterData.Core.Events.Abstractions;

public interface IFormEventHandlerResolver
{
    IFormEventHandler? GetFormEventHandler(string elementName);
}
