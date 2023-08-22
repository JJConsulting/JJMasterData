namespace JJMasterData.Core.FormEvents.Abstractions;

public interface IFormEventHandlerFactory
{
    IFormEventHandler? GetFormEvent(string elementName);
}
