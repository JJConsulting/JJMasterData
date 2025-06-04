#if !NET

namespace JJMasterData.Core.Http.Abstractions;

public interface IUrlHelper
{
    string Action(string action = null,
        string controller = null,
        object values = null);
}
#endif
