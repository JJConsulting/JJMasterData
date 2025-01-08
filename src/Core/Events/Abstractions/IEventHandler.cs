using System.Collections.Generic;

namespace JJMasterData.Core.Events.Abstractions;

public interface IEventHandler
{
    public string ElementName { get; }

#if NET
    IEnumerable<string> GetCustomizedFields()
    {
        yield break;
    }
#endif
}