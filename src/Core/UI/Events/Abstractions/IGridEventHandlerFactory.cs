using JJMasterData.Core.Web.FormEvents.Abstractions;

namespace JJMasterData.Core.Web.FormEvents.Factories;

public interface IGridEventHandlerFactory
{
    IGridEventHandler GetGridEventHandler(string elementName);
}