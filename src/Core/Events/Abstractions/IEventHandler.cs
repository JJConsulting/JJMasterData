using System.Collections.Generic;

namespace JJMasterData.Core.Events.Abstractions;

public interface IEventHandler
{
#if NET
    IEnumerable<string> GetCustomizedFields()
    {
        yield break;
    }
#endif
}