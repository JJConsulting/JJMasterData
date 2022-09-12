namespace JJMasterData.Core.FormEvents.Abstractions;

public interface IFormEventEngine
{
    IFormEvent GetFormEvent(string name);
}
