using JetBrains.Annotations;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.FormEvents;

public class FormEventHandlerFactory : EventHandlerFactoryBase<IFormEventHandler>, IFormEventHandlerFactory
{
    public FormEventHandlerFactory(IOptions<EventHandlerFactoryOptions> options,IServiceScopeFactory serviceScopeFactory) : base(options, serviceScopeFactory)
    {
    }

    public IFormEventHandler? GetFormEvent(string elementName)
    {
        return GetEventHandler(elementName);
    }
}