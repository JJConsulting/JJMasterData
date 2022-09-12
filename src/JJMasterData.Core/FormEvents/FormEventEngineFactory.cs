using JJMasterData.Commons.DI;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Core.FormEvents;

internal static class FormEventEngineFactory
{
    public static T GetEngine<T>() where T : IFormEventEngine
    {
        var engine = JJService.Provider?.GetService(typeof(T));

        if (engine is null or null)
            return default;
        
        return (T)engine;
    }
}
