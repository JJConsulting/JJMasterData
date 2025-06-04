namespace JJMasterData.Core.UI.Events.Abstractions;

public interface IGridEventHandlerResolver
{
    IGridEventHandler GetGridEventHandler(string elementName);
}