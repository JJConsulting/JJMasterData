namespace JJMasterData.Core.UI.Events.Abstractions;

public interface IGridEventHandlerFactory
{
    IGridEventHandler GetGridEventHandler(string elementName);
}