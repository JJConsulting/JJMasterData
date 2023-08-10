using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JJMasterData.Commons.Util;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Web.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.Web.FormEvents.Factories;

public class GridEventHandlerFactory : EventHandlerFactoryBase<IGridEventHandler>, IGridEventHandlerFactory
{
    public GridEventHandlerFactory(IOptions<EventHandlerFactoryOptions> options, IServiceScopeFactory serviceScopeFactory) : base(options, serviceScopeFactory)
    {
    }
    
    public IGridEventHandler GetGridEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}