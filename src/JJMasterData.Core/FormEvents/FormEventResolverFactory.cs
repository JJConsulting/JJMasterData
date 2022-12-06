using JJMasterData.Commons.DI;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Core.FormEvents;

internal static class FormEventResolverFactory
{
    public static IFormEventResolver GetResolver()
    {
        return JJService.Provider.GetService(typeof(IFormEventResolver)) as IFormEventResolver;
    }
}
