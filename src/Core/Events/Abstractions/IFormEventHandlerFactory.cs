#nullable enable

namespace JJMasterData.Core.Events.Abstractions;

public interface IFormEventHandlerFactory
{
    IFormEventHandler? GetFormEvent(string elementName);
}
