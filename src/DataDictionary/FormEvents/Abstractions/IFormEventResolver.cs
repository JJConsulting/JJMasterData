namespace JJMasterData.Core.FormEvents.Abstractions;

public interface IFormEventResolver
{
    IFormEvent GetFormEvent(string elementName);
}
