namespace JJMasterData.Web.Events.Abstractions;

public interface IGridEventHandlerResolver
{
    IGridEventHandler GetGridEventHandler(string elementName);
}