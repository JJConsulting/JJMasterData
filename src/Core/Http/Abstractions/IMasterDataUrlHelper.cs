using JetBrains.Annotations;

namespace JJMasterData.Core.Http.Abstractions;

public interface IMasterDataUrlHelper
{
#if NET
    string Action([AspMvcAction] string action = null, 
                  [AspMvcController] string controller = null, 
                  object values = null);
#else
    string Action(string action = null, 
                  string controller = null, 
                  object values = null);
#endif    
}